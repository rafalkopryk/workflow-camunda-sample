using Operations.BackOffice.Client.Data.ProcessInstances.Dto;

namespace Operations.BackOffice.Client.Data.ProcessInstances;
public interface IProcessInstanceService
{
    Task<SearchProcessInstanceQueryResponse> SearchProcessInstance(SearchProcessInstanceQuery? query);

    Task<string[]> GetProcessInstanceSequenceFlows(long processInstanceKey);
}
