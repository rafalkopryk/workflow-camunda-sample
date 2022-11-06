using Common.Zeebe;
using GatewayProtocol;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Application.Zeebe;

internal class ZeebeWorker<T> : BackgroundService
    where T : IZeebeTask
{
    private readonly Gateway.GatewayClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;

    public ZeebeWorker(Gateway.GatewayClient client, IServiceScopeFactory serviceScopeFactory, ILogger<ZeebeWorker<T>> logger)
    {
        _client = client;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var zeebeTask = typeof(T).GetZeebeTaskAttribute();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = 5,
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var jobType = zeebeTask.Type;
            var request = new ActivateJobsRequest
            {
                Type = jobType,
                MaxJobsToActivate = zeebeTask.MaxJobsToActivate,
                Timeout = zeebeTask.TimeoutInMs,
                RequestTimeout = zeebeTask.PollingTimeoutInMs,
            };

            try
            {
                using var call = _client.ActivateJobs(request, cancellationToken: stoppingToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    var activeatedJobs = response.Jobs.ToList();
                    await Parallel.ForEachAsync(activeatedJobs, parallelOptions, HandleJob);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(zeebeTask.PollIntervalInMs));
        }
    }

    private async ValueTask HandleJob(ActivatedJob? activatedJob, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            await Task.FromCanceled(cancellationToken);

        try
        {
            var internalJob = new InternalJob
            {
                BpmnProcessId = activatedJob.BpmnProcessId,
                CustomHeaders = activatedJob.CustomHeaders,
                Deadline = DateTimeOffset.FromUnixTimeMilliseconds(activatedJob.Deadline).LocalDateTime,
                Variables = activatedJob.Variables,
                ElementId = activatedJob.ElementId,
                ElementInstanceKey = activatedJob.ElementInstanceKey,
                Key = activatedJob.Key,
                ProcessDefinitionKey = activatedJob.ProcessDefinitionKey,
                ProcessDefinitionVersion = activatedJob.ProcessDefinitionVersion,
                ProcessInstanceKey = activatedJob.ProcessInstanceKey,
                Retries = activatedJob.Retries,
                Type = activatedJob.Type,
                Worker = activatedJob.Worker,
            };

            var instance = Activator.CreateInstance(typeof(T));
            ((IZeebeTask)instance).Job = internalJob;

            using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(instance, cancellationToken);

            await _client.CompleteJobAsync(new CompleteJobRequest
            {
                JobKey = activatedJob.Key,
            }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during subscribe job {jobType}", activatedJob.Type);

            await _client.FailJobAsync(new FailJobRequest
            {
                ErrorMessage = ex.Message,
                JobKey = activatedJob.Key,
                Retries = activatedJob.Retries - 1,
            });
        }
    }
}


