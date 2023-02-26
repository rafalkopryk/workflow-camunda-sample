namespace Camunda.Connector.SDK.Core.Impl.Outbound;

public record OutboundConnectorConfiguration
{
    public string Name { get; init; }
    public string Type { get; init; }
    public string[] InputVariables { get; init; }

    public override string ToString()
    {
        return "OutboundConnectorConfiguration{"
            + "name='"
            + Name
            + '\''
            + ", type='"
            + Type
            + '\''
            + ", inputVariables="
            + string.Join(',', InputVariables)
            + '}';
    }
}
