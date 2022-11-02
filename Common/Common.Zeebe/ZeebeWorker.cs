using Common.Zeebe;
using GatewayProtocol;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Application.Zeebe;

internal class ZeebeWorker : BackgroundService, IDisposable
{
    private readonly Gateway.GatewayClient _client;
    private readonly IZeebeJobHandlerProvider _zeebeJobHandlerProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;

    public ZeebeWorker(Gateway.GatewayClient client, IZeebeJobHandlerProvider zeebeJobHandlerProvider, IServiceScopeFactory serviceScopeFactory, ILogger<ZeebeWorker> logger)
    {
        _client = client;
        _zeebeJobHandlerProvider = zeebeJobHandlerProvider;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        foreach (var jobHandlerInfo in _zeebeJobHandlerProvider.GetJobs())
        {
            tasks.Add(Subscribe(jobHandlerInfo.Type, stoppingToken));
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(60));
        }
    }

    private async Task Subscribe(Type job, CancellationToken cancellationToken)
    {
        var zeebeJob = job.GetZeebeJobAttribute();
        while (!cancellationToken.IsCancellationRequested)
        {
            var jobType = zeebeJob.JobType;
            var request = new ActivateJobsRequest
            {
                Type = jobType,
                MaxJobsToActivate = zeebeJob.MaxJobsToActivate,
                Timeout = zeebeJob.TimeoutInMs,
                RequestTimeout = zeebeJob.PollingTimeoutInMs,
            };

            try
            {
                using var call = _client.ActivateJobs(request, cancellationToken: cancellationToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
                {
                    var jobTasks = response.Jobs.Select(activeatedJob => HandleJob(job, activeatedJob, cancellationToken));
                    await Task.WhenAll(jobTasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(zeebeJob.PollIntervalInMs));
        }
    }

    private async Task HandleJob(Type command, ActivatedJob? job, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            await Task.FromCanceled(cancellationToken);

        try
        {
            var internalJob = new InternalJob
            {
                BpmnProcessId = job.BpmnProcessId,
                CustomHeaders = job.CustomHeaders,
                Deadline = DateTimeOffset.FromUnixTimeMilliseconds(job.Deadline).LocalDateTime,
                Variables = job.Variables,
                ElementId = job.ElementId,
                ElementInstanceKey = job.ElementInstanceKey,
                Key = job.Key,
                ProcessDefinitionKey = job.ProcessDefinitionKey,
                ProcessDefinitionVersion = job.ProcessDefinitionVersion,
                ProcessInstanceKey = job.ProcessInstanceKey,
                Retries = job.Retries,
                Type = job.Type,
                Worker = job.Worker,
            };

            var instance = Activator.CreateInstance(command);
            ((IZeebeJob)instance).Job = internalJob;

            using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(instance, cancellationToken);

            await _client.CompleteJobAsync(new CompleteJobRequest
            {
                JobKey = job.Key,
            }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during subscribe job {jobType}", job.Type);

            await _client.FailJobAsync(new FailJobRequest
            {
                ErrorMessage = ex.Message,
                JobKey = job.Key,
                Retries = job.Retries - 1,
            });
        }
    }
}

public static class TimeSpanExtensions
{
    public static DateTime FromUtcNow(this TimeSpan timeSpan)
    {
        return DateTime.UtcNow + timeSpan;
    }
}


