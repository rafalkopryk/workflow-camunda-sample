namespace Operations.BackOffice.Client.Data.ProcessInstances
{

    public interface IProcessInstanceService
    {
        Task<ProcessInstanceDto[]> GetProcessInstances(string bpmnProcessId);

        Task<ProcessInstanceDto[]> GetProcessInstances(long processDefinitionKey);

        Task<ProcessInstanceDto> GetProcessInstance(long processInstanceKey);

        Task<string[]> GetProcessInstanceSequenceFlows(long processInstanceKey);
    }
}
