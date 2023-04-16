namespace Camunda.Connector.SDK.Core.Api.Annotation;

public class InboundConnectorAttribute : Attribute
{
    public string Name { get; set; }
    public string Type { get; set; }
}
