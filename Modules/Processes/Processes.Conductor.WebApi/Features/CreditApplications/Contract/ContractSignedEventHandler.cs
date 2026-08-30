using Applications.Contracts.Events;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Contract;

[WolverineHandler]
public class ContractSignedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(ContractSigned message, CancellationToken cancellationToken)
    {
        return workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitContractSignedTask);
    }
}
