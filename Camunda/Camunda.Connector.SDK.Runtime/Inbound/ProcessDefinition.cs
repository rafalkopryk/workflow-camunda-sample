namespace Camunda.Connector.SDK.Runtime.Inbound;

public record ProcessDefinition
(
    long Key,
    string Name,
    long Version,
    string BpmnProcessId
 );
