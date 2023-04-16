namespace Camunda.Connector.SDK.Runtime.Inbound.Importer.File;

public interface IBpmnProvider
{
    Task<byte[]> GetBpmn(ProcessDefinition processDefinition);
}
