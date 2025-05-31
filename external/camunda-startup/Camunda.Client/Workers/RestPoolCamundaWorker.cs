using Camunda.Client.Jobs;
using Camunda.Client.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client.Workers;

internal class RestPoolCamundaWorker<T>(
    ICamundaClientRest client,
    InternalJobWorkerConfiguration jobWorkerConfiguration,
    JobExecutor jobExecutor,
    ILogger<RestPoolCamundaWorker<T>> logger
    ) : BackgroundService where T : IJobHandler
{
    private readonly ICamundaClientRest _client = client;
    private readonly ILogger _logger = logger;
    private readonly InternalJobWorkerConfiguration _jobWorkerConfiguration = jobWorkerConfiguration;
    private readonly JobExecutor _jobExecutor = jobExecutor;
    private const int MAX_DEGREE_OF_PARALLELISM = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobType = _jobWorkerConfiguration.Type;
        var request = new JobActivationRequest
        {
            Type = jobType,
            Timeout = _jobWorkerConfiguration.TimeoutInMs,
            TenantIds = [.._jobWorkerConfiguration.TenantIds ],
            MaxJobsToActivate = _jobWorkerConfiguration.PollingMaxJobsToActivate,
            RequestTimeout = _jobWorkerConfiguration.PollingRequestTimeoutInMs,
        };

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = Diagnostics.Consumer.StartPoolActivatedJobs(jobType);
            try
            {
                var response = await _client.ActivationAsync(request, stoppingToken);
                var jobs = response.Jobs.ToArray();
                var jobsCount = jobs.Length;
                
                if (jobsCount == 1)
                {
                    await _jobExecutor.HandleJob<T>(Map(jobs.First()), _jobWorkerConfiguration, CancellationToken.None);
                }
                else if (jobsCount > 1)
                {
                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = CancellationToken.None,
                        MaxDegreeOfParallelism = jobsCount > MAX_DEGREE_OF_PARALLELISM
                            ? MAX_DEGREE_OF_PARALLELISM
                            : jobsCount,
                    };

                    await Parallel.ForEachAsync(jobs, parallelOptions,
                        async (job, cancellationToken) =>
                            await _jobExecutor.HandleJob<T>(Map(job), _jobWorkerConfiguration, cancellationToken));                
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);

                activity?.AddException(ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(_jobWorkerConfiguration.PollingDelayInMs), stoppingToken);
        }
    }

    private static IJob Map(ActivatedJob activatedJob)
    {
        return new InternalJob
        {
            BpmnProcessId = activatedJob.ProcessDefinitionId,
            CustomHeaders = Newtonsoft.Json.JsonConvert.SerializeObject(activatedJob.CustomHeaders),
            Deadline = DateTimeOffset.FromUnixTimeMilliseconds(activatedJob.Deadline).ToLocalTime(),
            Variables = Newtonsoft.Json.JsonConvert.SerializeObject(activatedJob.Variables),
            ElementId = activatedJob.ElementId,
            ElementInstanceKey = activatedJob.ElementInstanceKey,
            Key = activatedJob.JobKey,
            ProcessDefinitionKey = activatedJob.ProcessDefinitionKey,
            ProcessDefinitionVersion = activatedJob.ProcessDefinitionVersion,
            ProcessInstanceKey = activatedJob.ProcessInstanceKey,
            Retries = activatedJob.Retries,
            Type = activatedJob.Type,
            Worker = activatedJob.Worker,
        };
    }
}
