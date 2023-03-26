using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Microsoft.Extensions.Configuration;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer.Mock;

internal class MockProcessDefinitionInspector : IProcessDefinitionInspector
{
    private readonly IConfiguration _configuration;

    public MockProcessDefinitionInspector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<InboundConnectorProperties[]> FindInboundConnectors(ProcessDefinition processDefinition)
    {
        await Task.Yield();
        var inboundConnectorPropertiesOptions = _configuration.GetSection("InboundConnectorProperties").Get<InboundConnectorPropertiesOptions[]>();

        return inboundConnectorPropertiesOptions.Select(x => new InboundConnectorProperties
        {
            BpmnProcessId = x.BpmnProcessId,
            CorrelationPoint = new MessageCorrelationPoint(x.CorrelationPoint.MessageName, x.Properties.FirstOrDefault(x => x.Key == "inbound.correlationKeyMapping").Value),
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
}