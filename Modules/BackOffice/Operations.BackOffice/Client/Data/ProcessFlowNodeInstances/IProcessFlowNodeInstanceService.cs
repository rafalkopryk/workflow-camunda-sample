using Operations.BackOffice.Client.Data.ProcessFlowNodeInstances.Dto;

namespace Operations.BackOffice.Client.Data.ProcessFlowNodeInstances;
public interface IProcessFlowNodeInstanceService
{
    Task<SearchFlowNodeInstancesQueryResponse> SearchProcessFlowNodeInstance(SearchFlowNodeInstancesQuery? query);
}
