using Common.Application.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Impl.Builder;

namespace Common.Application.Zeebe;

internal class ZeebeService : IZeebeService
{
    private readonly IZeebeClient _client;
    private readonly ILogger<ZeebeService> _logger;

    public ZeebeService(ILogger<ZeebeService> logger, IOptions<ZeebeOptions> zeebeOptions)
    {
        var options = zeebeOptions.Value;
        _logger = logger;
        _client = options.Cloud
            ? CamundaCloudClientBuilder
                .Builder()
                .UseClientId(options.Client.Id)
                .UseClientSecret(options.Client.Secret)
                .UseContactPoint(options.Address)
                .Build()
            : ZeebeClient.Builder()
                .UseGatewayAddress(options.Address)
                .UsePlainText()
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