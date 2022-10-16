using Common.Application.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Builder;

namespace Common.Application.Zeebe;

public interface IZeebeService
{
    Task<long> StartProcessInstance(string bpmProcessId, object veriables);

    Task PublishMessage(string messageName, string correlationKey, CancellationToken cancellationToken);

    Task SetVeriables(IJob job, object variables, CancellationToken cancellationToken);
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

    public async Task PublishMessage(string messageName, string correlationKey, CancellationToken cancellationToken)
    {
        var publishMessage = _client.NewPublishMessageCommand();
        await publishMessage.MessageName(messageName).CorrelationKey(correlationKey).SendWithRetry(TimeSpan.FromSeconds(60), cancellationToken);
    }

    public async Task SetVeriables(IJob job, object veriables, CancellationToken cancellationToken)
    {
        var veriablesText = JsonSerializer.Serialize(veriables, JsonSerializerCustomOptions.CamelCase);
        var setVariablesCommand = _client.NewSetVariablesCommand(job.ElementInstanceKey);
        await setVariablesCommand.Variables(veriablesText).SendWithRetry(TimeSpan.FromSeconds(60), cancellationToken);
    }

    public async Task<long> StartProcessInstance(string bpmProcessId, object veriables)
    {
        var veriablesText = JsonSerializer.Serialize(veriables, JsonSerializerCustomOptions.CamelCase);
        var instance = await _client.NewCreateProcessInstanceCommand()
            .BpmnProcessId(bpmProcessId)
            .LatestVersion()
            .Variables(veriablesText)
            .Send();

        return instance.ProcessInstanceKey;
    }
}