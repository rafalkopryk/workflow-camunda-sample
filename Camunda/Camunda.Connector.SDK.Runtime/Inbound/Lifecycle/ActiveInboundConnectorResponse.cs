namespace Camunda.Connector.SDK.Runtime.Inbound.Lifecycle;

public record ActiveInboundConnectorResponse
(
    string BpmnProcessId,
    string Type,
    Dictionary<string, object> Data
);
