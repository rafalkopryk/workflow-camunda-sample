namespace Processes.Application.Utils.Importer.File;

public interface IBpmnProvider
{
    Task<byte[]> GetBpmn(ProcessDefinition processDefinition);
}
