using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Microsoft.Extensions.Configuration;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer;

public class MockProcessDefinitionInspector : IProcessDefinitionInspector
{
    private readonly IConfiguration _configuration;

    public MockProcessDefinitionInspector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public InboundConnectorProperties[] FindInboundConnectors(ProcessDefinition processDefinition)
    {

        var inboundConnectorPropertiesOptions = _configuration.GetSection("InboundConnectorProperties").Get<InboundConnectorPropertiesOptions[]>();

        return inboundConnectorPropertiesOptions.Select(x => new InboundConnectorProperties
        {
            BpmnProcessId = x.BpmnProcessId,
            CorrelationPoint = new MessageCorrelationPoint(x.CorrelationPoint.MessageName, x.CorrelationPoint.CorrelationKeyExpression),
            Properties = x.Properties
        }).ToArray();
    }
}

public record InboundConnectorPropertiesOptions
{
    public Dictionary<string, string> Properties { get; init; }
    public MessageCorrelationOptions CorrelationPoint { get; init; }
    public string BpmnProcessId { get; init; }
}

public record MessageCorrelationOptions
{
    public string MessageName { get; init; }
    public string CorrelationKeyExpression { get; init; }
}