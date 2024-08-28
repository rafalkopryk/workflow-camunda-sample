using Camunda.Client.Jobs;
using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client.Workers;

internal class GrpcPoolCamundaWorker<T>(
    Gateway.GatewayClient client,
    JobWorkerConfiguration serviceTaskConfiguration,
    JobExecutor jobExecutor,
    ILogger<GrpcPoolCamundaWorker<T>> logger
    ) : BackgroundService where T : IJobHandler
{
    private readonly Gateway.GatewayClient _client = client;
    private readonly ILogger _logger = logger;
    private readonly JobWorkerConfiguration _serviceTaskConfiguration = serviceTaskConfiguration;
    private readonly JobExecutor _jobExecutor = jobExecutor;
    private const int MAX_DEGREE_OF_PARALLELISM = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobType = _serviceTaskConfiguration.Type;
        var request = new ActivateJobsRequest
        {
            Type = jobType,
            Timeout = _serviceTaskConfiguration.TimeoutInMs,
            TenantIds = { _serviceTaskConfiguration.TenatIds },
            MaxJobsToActivate = _serviceTaskConfiguration.PoolingMaxJobsToActivate,
            RequestTimeout = _serviceTaskConfiguration.PoolingRequestTimeoutInMs,
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

                    if (jobsCount > 1)
                    {
                        var parallelOptions = new ParallelOptions
                        {
                            CancellationToken = CancellationToken.None,
                            MaxDegreeOfParallelism = jobsCount > MAX_DEGREE_OF_PARALLELISM
                                ? MAX_DEGREE_OF_PARALLELISM
                                : jobsCount,
                        };

                        await Parallel.ForEachAsync(jobs, parallelOptions, async (job, cancellationToken) => await _jobExecutor.HandleJob<T>(GrpcCamundaWorkerHelpers.Map(job), _serviceTaskConfiguration, cancellationToken));
                    }
                    else
                    {
                        await _jobExecutor.HandleJob<T>(GrpcCamundaWorkerHelpers.Map(jobs.First()), _serviceTaskConfiguration, CancellationToken.None);
                    }
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
}
