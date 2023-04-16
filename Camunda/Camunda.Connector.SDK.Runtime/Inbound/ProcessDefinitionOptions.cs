namespace Camunda.Connector.SDK.Runtime.Inbound;

public record ProcessDefinitionOptions
{
    public ProcessDefinition[] ProcessDefinitions { get; init; }
}
