using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Camunda.Client;

internal class ZeebeWorker<T> : BackgroundService
    where T : IJobHandler
{
    private readonly Gateway.GatewayClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;
    private readonly ServiceTaskConfiguration _serviceTaskConfiguration;

    public ZeebeWorker(Gateway.GatewayClient client, IServiceScopeFactory serviceScopeFactory, ServiceTaskConfiguration serviceTaskConfiguration, ILogger<ZeebeWorker<T>> logger)
    {
        _client = client;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _serviceTaskConfiguration = serviceTaskConfiguration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            var jobType = _serviceTaskConfiguration.Type;
            var request = new StreamActivatedJobsRequest
            {
                Type = jobType,
                Timeout = _serviceTaskConfiguration.TimeoutInMs,
                TenantIds = { _serviceTaskConfiguration.TenatIds },
            };

            using var activity = Diagnostics.Consumer.Start(jobType);

            try
            {
                using var call = _client.StreamActivatedJobs(request, cancellationToken: stoppingToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    await HandleJob(response, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);
                
                activity?.RecordException(ex);
            }
        }
    }

    private async ValueTask HandleJob(ActivatedJob activatedJob, CancellationToken cancellationToken)
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

            using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            var jobClient = serviceScope.ServiceProvider.GetRequiredService<IJobClient>();
            var handler = serviceScope.ServiceProvider.GetRequiredService<T>();
            await handler.Handle(jobClient, internalJob, cancellationToken);

            if (!_serviceTaskConfiguration.AutoComplate)
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

            var retryBackOff = _serviceTaskConfiguration.RetryBackOffInMs?.TakeLast(activatedJob.Retries).FirstOrDefault() ?? 500;
            await _client.FailJobAsync(new FailJobRequest
            {
                ErrorMessage = ex.Message,
                JobKey = activatedJob.Key,
                Retries = activatedJob.Retries - 1,
                RetryBackOff = retryBackOff,
            });
        }
    }
}


