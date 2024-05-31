using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client;

internal class StreamZeebeWorker<T>(
    Gateway.GatewayClient client,
    ServiceTaskConfiguration serviceTaskConfiguration,
    JobExecutor jobExecutor,
    ILogger<StreamZeebeWorker<T>> logger
    ) : BackgroundService where T : IJobHandler
{
    private readonly Gateway.GatewayClient _client = client;
    private readonly ILogger _logger = logger;
    private readonly ServiceTaskConfiguration _serviceTaskConfiguration = serviceTaskConfiguration;
    private readonly JobExecutor _jobExecutor = jobExecutor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobType = _serviceTaskConfiguration.Type;
        var request = new StreamActivatedJobsRequest
        {
            Type = jobType,
            Timeout = _serviceTaskConfiguration.TimeoutInMs,
            TenantIds = { _serviceTaskConfiguration.TenatIds },
        };

        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = Diagnostics.Consumer.StartStreamActivatedJobs(jobType);
            try
            {
                using var call = _client.StreamActivatedJobs(
                    request,
                    deadline: TimeProvider.System.GetUtcNow().AddSeconds(_serviceTaskConfiguration.StreamTimeoutInSec).UtcDateTime,
                    cancellationToken: stoppingToken);
                await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    await _jobExecutor.HandleJob<T>(response, _serviceTaskConfiguration, CancellationToken.None);
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
            {
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during read stream {jobType}", jobType);

                activity?.AddException(ex);

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}


