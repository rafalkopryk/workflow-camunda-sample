using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client;

internal class ZeebeWorker<T> : BackgroundService
    where T : IJobHandler
{
    private readonly Gateway.GatewayClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;
    private static int s_currentJobsActive = 0;

    public ZeebeWorker(Gateway.GatewayClient client, IServiceScopeFactory serviceScopeFactory, ILogger<ZeebeWorker<T>> logger)
    {
        _client = client;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var zeebeWorker = typeof(T).GetZeebeWorkerAttribute();
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = stoppingToken,
            MaxDegreeOfParallelism = 5,
        };

        var thresholdJobsActivation = zeebeWorker!.MaxJobsToActivate * 0.6;

        while (!stoppingToken.IsCancellationRequested)
        {
            var jobCount = zeebeWorker.MaxJobsToActivate - s_currentJobsActive;
            var jobType = zeebeWorker.Type;
            var request = new ActivateJobsRequest
            {
                Type = jobType,
                MaxJobsToActivate = jobCount,
                Timeout = zeebeWorker.TimeoutInMs,
                RequestTimeout = zeebeWorker.PollingTimeoutInMs,
            };

            try
            {
                using var call = _client.ActivateJobs(request, cancellationToken: stoppingToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    var source = response.Jobs.Select(x => (x, zeebeWorker)).ToList();
                    await Parallel.ForEachAsync(source, parallelOptions, HandleJob);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);
                await Task.Delay(TimeSpan.FromMilliseconds(zeebeWorker.PollIntervalInMs));
            }

            if (s_currentJobsActive > thresholdJobsActivation)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(zeebeWorker.PollIntervalInMs));
            }
        }
    }

    private async ValueTask HandleJob((ActivatedJob? ActivatedJob, ZeebeWorkerAttribute ZeebeWorker) source, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            await Task.FromCanceled(cancellationToken);


        Interlocked.Increment(ref s_currentJobsActive);
        var activatedJob = source.ActivatedJob;
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

            using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            var jobClient = serviceScope.ServiceProvider.GetRequiredService<IJobClient>();
            var handler = serviceScope.ServiceProvider.GetRequiredService<T>();
            await handler.Handle(jobClient, internalJob, cancellationToken);

            if (!source.ZeebeWorker.AutoComplate)
            {
                return;
            }

            await _client.CompleteJobAsync(new CompleteJobRequest
            {
                JobKey = activatedJob.Key,
            }, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during subscribe job {jobType}", activatedJob.Type);

            var retryBackOff = source.ZeebeWorker.RetryBackOffInMs?.TakeLast(activatedJob.Retries).FirstOrDefault() ?? 500;
            await _client.FailJobAsync(new FailJobRequest
            {
                ErrorMessage = ex.Message,
                JobKey = activatedJob.Key,
                Retries = activatedJob.Retries - 1,
                RetryBackOff = retryBackOff,
            });
        }
        finally
        {
            Interlocked.Decrement(ref s_currentJobsActive);
        }
    }
}


