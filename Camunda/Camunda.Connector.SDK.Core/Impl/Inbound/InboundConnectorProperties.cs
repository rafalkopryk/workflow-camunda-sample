namespace Camunda.Connector.SDK.Core.Impl.Inbound;

using static Camunda.Connector.SDK.Core.Impl.Constants;

public record InboundConnectorProperties
{
    public string Type => Properties?.FirstOrDefault(x => string.Equals(x.Key, INBOUND_TYPE_KEYWORD, StringComparison.OrdinalIgnoreCase)).Value;
    public Dictionary<string, string> Properties { get; init; }
    public ProcessCorrelationPoint CorrelationPoint { get; init; }
    public string BpmnProcessId { get; init; }
    public int Version { get; init; }
    public long ProcessDefinitionKey { get; init; }

    public string? GetCorrelationPointId()
    {
        return CorrelationPoint?.GetId();
    }

    public string? GetProperty(string propertyName)
    {
        return Properties?.FirstOrDefault(x => string.Equals(x.Key, propertyName, StringComparison.OrdinalIgnoreCase)).Value;
    }

    public string GetRequiredProperty(string propertyName)
    {
        var property = GetProperty(propertyName);
        return property is null
            ? throw new InvalidOperationException(
                "Required inbound connector property '" + propertyName + "' is missing.")
            : property;
    }
}
