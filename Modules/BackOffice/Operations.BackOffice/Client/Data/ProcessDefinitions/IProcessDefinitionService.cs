using Operations.BackOffice.Client.Data.ProcessDefinitions.Dto;

namespace Operations.BackOffice.Client.Data.ProcessDefinitions
{
    public interface IProcessDefinitionService
    {
        Task<SearchProcessDefinitionsQueryResponse> SearchProcessDefinitions(SearchProcessDefinitionsQuery query);

        Task<string> GetProcessDefinitionXml(long key);
    }
}
