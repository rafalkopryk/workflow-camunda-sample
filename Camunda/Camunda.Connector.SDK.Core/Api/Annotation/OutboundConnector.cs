namespace Camunda.Connector.SDK.Core.Api.Annotation;

public class OutboundConnectorAttribute : Attribute
{
    public string Name { get; set; }
    public string[] InputVariables { get; set; }
    public string Type { get; set; }
}

