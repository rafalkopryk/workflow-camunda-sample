using GatewayProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Camunda.Client;

internal class JobExecutor(
    Gateway.GatewayClient client,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<JobExecutor> logger)
{
    private readonly Gateway.GatewayClient _client = client;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger _logger = logger;

    public async ValueTask HandleJob<T>(ActivatedJob activatedJob, ServiceTaskConfiguration serviceTaskConfiguration, CancellationToken cancellationToken = default) where T : IJobHandler
    {
        if (cancellationToken.IsCancellationRequested)
            await Task.FromCanceled(cancellationToken);

        using var activity = Diagnostics.Consumer.StartHandleTask(activatedJob.ElementId);
        try
        {
            var internalJob = new InternalJob
            {
                BpmnProcessId = activatedJob.BpmnProcessId,
                CustomHeaders = activatedJob.CustomHeaders,
                Deadline = DateTimeOffset.FromUnixTimeMilliseconds(activatedJob.Deadline).ToLocalTime(),
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

            activity?.AddtOpenTelemetryTags(internalJob);

            using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
            var jobClient = serviceScope.ServiceProvider.GetRequiredService<IJobClient>();
            var handler = serviceScope.ServiceProvider.GetRequiredService<T>();
            await handler.Handle(jobClient, internalJob, cancellationToken);

            if (!serviceTaskConfiguration.AutoComplate)
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
            activity?.AddException(ex);

            _logger.LogError(ex, "Error during subscribe job {jobType}", activatedJob.Type);

            try
            {
                var retryBackOff = serviceTaskConfiguration.RetryBackOffInMs?.TakeLast(activatedJob.Retries).FirstOrDefault() ?? 500;
                await _client.FailJobAsync(new FailJobRequest
                {
                    ErrorMessage = ex.Message,
                    JobKey = activatedJob.Key,
                    Retries = activatedJob.Retries - 1,
                    RetryBackOff = retryBackOff,
                });
            }
            catch (Exception)
            {
                activity?.AddException(ex);
                _logger.LogError(ex, "Error during fail job {jobType}", activatedJob.Type);
                throw;
            }
        }
    }
}


