using Camunda.Client.Jobs;
using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client.Workers;

internal class GrpcPoolCamundaWorker<T>(
    Gateway.GatewayClient client,
    InternalJobWorkerConfiguration jobWorkerConfiguration,
    JobExecutor jobExecutor,
    ILogger<GrpcPoolCamundaWorker<T>> logger
    ) : BackgroundService where T : IJobHandler
{
    private readonly Gateway.GatewayClient _client = client;
    private readonly ILogger _logger = logger;
    private readonly InternalJobWorkerConfiguration _jobWorkerConfiguration = jobWorkerConfiguration;
    private readonly JobExecutor _jobExecutor = jobExecutor;
    private const int MAX_DEGREE_OF_PARALLELISM = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobType = _jobWorkerConfiguration.Type;
        var request = new ActivateJobsRequest
        {
            Type = jobType,
            Timeout = _jobWorkerConfiguration.TimeoutInMs,
            TenantIds = { _jobWorkerConfiguration.TenatIds },
            MaxJobsToActivate = _jobWorkerConfiguration.PoolingMaxJobsToActivate,
            RequestTimeout = _jobWorkerConfiguration.PoolingRequestTimeoutInMs,
        };

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = Diagnostics.Consumer.StartPoolActivatedJobs(jobType);
            try
            {
                using var call = _client.ActivateJobs(
                    request,
                    cancellationToken: stoppingToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    var jobs = response.Jobs.ToArray();
                    var jobsCount = jobs.Length;

                    if (jobsCount == 0)
                    {
                        continue;
                    }

                    if (jobsCount > 1)
                    {
                        var parallelOptions = new ParallelOptions
                        {
                            CancellationToken = CancellationToken.None,
                            MaxDegreeOfParallelism = jobsCount > MAX_DEGREE_OF_PARALLELISM
                                ? MAX_DEGREE_OF_PARALLELISM
                                : jobsCount,
                        };

                        await Parallel.ForEachAsync(jobs, parallelOptions, async (job, cancellationToken) => await _jobExecutor.HandleJob<T>(GrpcCamundaWorkerHelpers.Map(job), _jobWorkerConfiguration, cancellationToken));
                    }
                    else
                    {
                        await _jobExecutor.HandleJob<T>(GrpcCamundaWorkerHelpers.Map(jobs.First()), _jobWorkerConfiguration, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);

                activity?.AddException(ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(_jobWorkerConfiguration.PoolingDelayInMs), stoppingToken);
        }
    }
}
