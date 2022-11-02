using GatewayProtocol;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.Zeebe;

internal class ZeebeService : IZeebeService
{
    private readonly Gateway.GatewayClient _client;
    private readonly ILogger<ZeebeService> _logger;

    public ZeebeService(ILogger<ZeebeService> logger, Gateway.GatewayClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task CompleteJob(IJob job, CancellationToken cancellationToken)
    {
        var result = await _client.CompleteJobAsync(new CompleteJobRequest
        {
            JobKey = job.Key,
        }, cancellationToken: cancellationToken);
    }

    public async Task PublishMessage(string messageName, string correlationKey, CancellationToken cancellationToken)
    {
        var result = await _client.PublishMessageAsync(new PublishMessageRequest
        {
            Name = messageName,
            CorrelationKey = correlationKey,
        }, cancellationToken: cancellationToken);
    }

    public async Task SetVeriables(long elementInstanceKey, object veriables, CancellationToken cancellationToken)
    {
        var veriablesText = JsonSerializer.Serialize(veriables, JsonSerializerCustomOptions.CamelCase);
        var result = await _client.SetVariablesAsync(new SetVariablesRequest
        {
            ElementInstanceKey = elementInstanceKey,
            Variables = veriablesText,
        });
    }

    public async Task<long> StartProcessInstance(string bpmProcessId, object veriables)
    {
        var veriablesText = JsonSerializer.Serialize(veriables, JsonSerializerCustomOptions.CamelCase);
        var instance = await _client.CreateProcessInstanceAsync(new CreateProcessInstanceRequest
        {
            BpmnProcessId = bpmProcessId,
            Variables = veriablesText,
            Version = -1
        });

        return instance.ProcessInstanceKey;
    }
}
