using Camunda.Connector.SDK.Core.Impl.Inbound;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer;

public interface IProcessDefinitionInspector
{
    InboundConnectorProperties[] FindInboundConnectors(ProcessDefinition processDefinition);
}
