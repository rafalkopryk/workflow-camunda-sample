using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Builder;

namespace Common.Application.Zeebe;

public interface IZeebeService
{
    Task<ITopology> Status();

    Task<long> StartProcessInstance(string bpmProcessId);

    Task CompleteJob(IJob job, CancellationToken cancellationToken);
}

internal class ZeebeService : IZeebeService
{
    private readonly IZeebeClient _client;
    private readonly ILogger<ZeebeService> _logger;

    public ZeebeService(ILogger<ZeebeService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var address = configuration.GetSection("ZEEBE:ADDRESS").Value;
        var clientId = configuration.GetSection("ZEEBE:CLIENT:ID").Value;
        var clientSecret = configuration.GetSection("ZEEBE:CLIENT:SECRET").Value;

        _client = CamundaCloudClientBuilder
            .Builder()
            .UseClientId(clientId)
            .UseClientSecret(clientSecret)
            .UseContactPoint(address)
            .Build();
    }

    public async Task CompleteJob(IJob job, CancellationToken cancellationToken)
    {
        var completeJobCommand = _client.NewCompleteJobCommand(job.Key);
        await completeJobCommand.SendWithRetry(TimeSpan.FromSeconds(60), cancellationToken);
    }

    public async Task<long> StartProcessInstance(string bpmProcessId)
    {
        var instance = await _client.NewCreateProcessInstanceCommand()
            .BpmnProcessId(bpmProcessId)
            .LatestVersion()
            .Send();

        return instance.ProcessInstanceKey;
    }

    public async Task<ITopology> Status()
    {
        return await _client.TopologyRequest().Send();
    }
}