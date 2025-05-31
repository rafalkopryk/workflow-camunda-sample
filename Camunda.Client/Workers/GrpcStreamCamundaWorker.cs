using Camunda.Client.Jobs;
using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client.Workers;

internal class GrpcStreamCamundaWorker<T>(
    Gateway.GatewayClient client,
    InternalJobWorkerConfiguration jobWorkerConfiguration,
    JobExecutor jobExecutor,
    ILogger<GrpcStreamCamundaWorker<T>> logger
    ) : BackgroundService where T : IJobHandler
{
    private readonly Gateway.GatewayClient _client = client;
    private readonly ILogger _logger = logger;
    private readonly InternalJobWorkerConfiguration _jobWorkerConfiguration = jobWorkerConfiguration;
    private readonly JobExecutor _jobExecutor = jobExecutor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobType = _jobWorkerConfiguration.Type;
        var request = new StreamActivatedJobsRequest
        {
            Type = jobType,
            Timeout = _jobWorkerConfiguration.TimeoutInMs,
            TenantIds = { _jobWorkerConfiguration.TenantIds },
        };

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = Diagnostics.Consumer.StartStreamActivatedJobs(jobType);
            try
            {
                using var call = _client.StreamActivatedJobs(
                    request,
                    deadline: TimeProvider.System.GetUtcNow().AddSeconds(_jobWorkerConfiguration.StreamTimeoutInSec).UtcDateTime,
                    cancellationToken: stoppingToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    await _jobExecutor.HandleJob<T>(GrpcCamundaWorkerHelpers.Map(response), _jobWorkerConfiguration, CancellationToken.None);
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.DeadlineExceeded)
            {
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);

                activity?.AddException(ex);

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}


