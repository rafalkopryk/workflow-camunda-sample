using Operations.BackOffice.Client.Data.ProcessDefinitions.Dto;

namespace Operations.BackOffice.Client.Data.ProcessDefinitions
{
    public interface IProcessDefinitionService
    {
        Task<ProcessDefinitionDto[]> GetProcessDefinitions();

        Task<ProcessDefinitionDto[]> GetProcessDefinitions(string bpmnProcessId);


        Task<string> GetProcessDefinitionXml(long key);
    }
}
