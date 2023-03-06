using Camunda.Connector.SDK.Core.Api.Inbound;

namespace Camunda.Connector.SDK.Core.Impl.Inbound;

public record InboundConnectorProperties
{
    public string Type => Properties?.FirstOrDefault(x => string.Equals(x.Key, "inbound.type", StringComparison.OrdinalIgnoreCase)).Value;
    public Dictionary<string, string> Properties { get; init; }
    public ProcessCorrelationPoint CorrelationPoint { get; init; }
    public string BpmnProcessId { get; init; }
    public int Version { get; init; }
    public long ProcessDefinitionKey { get; init; }

    public string GetCorrelationPointId()
    {
        return CorrelationPoint?.GetId();
    }
}
