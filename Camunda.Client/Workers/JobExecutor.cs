using System.Diagnostics;
using Camunda.Client.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Camunda.Client.Workers;

internal class JobExecutor(
    IJobClient client,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<JobExecutor> logger)
{
    private readonly IJobClient _client = client;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger _logger = logger;

    public async ValueTask HandleJob<T>(IJob activatedJob, InternalJobWorkerConfiguration jobWorkerConfiguration,
        CancellationToken cancellationToken = default) where T : IJobHandler
    {
        if (cancellationToken.IsCancellationRequested)
            await Task.FromCanceled(cancellationToken);

        var parent = Activity.Current;
        var link = new ActivityLink(Activity.Current.Context);

        Activity.Current = null;
        
        try
        {
            using var activity = Diagnostics.Consumer.StartHandleTask(activatedJob.ElementId, link);
            try
            {
                activity?.AddtOpenTelemetryTags(activatedJob);

                using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
                var jobClient = serviceScope.ServiceProvider.GetRequiredService<IJobClient>();
                var handler = serviceScope.ServiceProvider.GetRequiredService<T>();
                await handler.Handle(jobClient, activatedJob, cancellationToken);

                if (!jobWorkerConfiguration.AutoComplete)
                {
                    return;
                }

                await _client.CompleteJobCommand(activatedJob);
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);

                _logger.LogError(ex, "Error during subscribe job {jobType}", activatedJob.Type);

                try
                {
                    var retryBackOff =
                        jobWorkerConfiguration.RetryBackOffInMs?.TakeLast(activatedJob.Retries).FirstOrDefault() ?? 500;

                    await _client.FailCommand(activatedJob.Key, ex.Message, activatedJob.Retries - 1, retryBackOff);
                }
                catch (Exception)
                {
                    activity?.AddException(ex);
                    _logger.LogError(ex, "Error during fail job {jobType}", activatedJob.Type);
                    throw;
                }
            }
        }
        finally
        {
            Activity.Current = parent;
        }
    }
}