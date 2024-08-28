using Camunda.Client.Jobs;
using Camunda.Client.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Camunda.Client.Workers;

internal class RestPoolCamundaWorker<T>(
    CamundaClientRest client,
    JobWorkerConfiguration serviceTaskConfiguration,
    JobExecutor jobExecutor,
    ILogger<GrpcPoolCamundaWorker<T>> logger
    ) : BackgroundService where T : IJobHandler
{
    private readonly CamundaClientRest _client = client;
    private readonly ILogger _logger = logger;
    private readonly JobWorkerConfiguration _serviceTaskConfiguration = serviceTaskConfiguration;
    private readonly JobExecutor _jobExecutor = jobExecutor;
    private const int MAX_DEGREE_OF_PARALLELISM = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobType = _serviceTaskConfiguration.Type;
        var request = new JobActivationRequest
        {
            Type = jobType,
            Timeout = _serviceTaskConfiguration.TimeoutInMs,
            TenantIds = [.._serviceTaskConfiguration.TenatIds ],
            MaxJobsToActivate = _serviceTaskConfiguration.PoolingMaxJobsToActivate,
            RequestTimeout = _serviceTaskConfiguration.PoolingRequestTimeoutInMs,
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

                if (jobsCount > 1)
                {
                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = CancellationToken.None,
                        MaxDegreeOfParallelism = jobsCount > MAX_DEGREE_OF_PARALLELISM
                            ? MAX_DEGREE_OF_PARALLELISM
                            : jobsCount,
                    };

                    await Parallel.ForEachAsync(jobs, parallelOptions, async (job, cancellationToken) => await _jobExecutor.HandleJob<T>(Map(job), _serviceTaskConfiguration, cancellationToken));
                }
                else
                {
                    await _jobExecutor.HandleJob<T>(Map(jobs.First()), _serviceTaskConfiguration, CancellationToken.None);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);

                activity?.AddException(ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(_serviceTaskConfiguration.PoolingDelayInMs), stoppingToken);
        }
    }

    private static IJob Map(ActivatedJob activatedJob)
    {
        return new InternalJob
        {
            BpmnProcessId = activatedJob.BpmnProcessId,
            CustomHeaders = JsonSerializer.Serialize(activatedJob.CustomHeaders, JsonSerializerCustomOptions.CamelCase),
            Deadline = DateTimeOffset.FromUnixTimeMilliseconds(activatedJob.Deadline).ToLocalTime(),
            Variables = JsonSerializer.Serialize(activatedJob.Variables, JsonSerializerCustomOptions.CamelCase),
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
    }
}
