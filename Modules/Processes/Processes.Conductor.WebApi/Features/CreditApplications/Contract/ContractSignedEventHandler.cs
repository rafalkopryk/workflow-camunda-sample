using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Contract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

public class ContractSignedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(ContractSigned message, CancellationToken cancellationToken)
    {
        return workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitContractSignedTask);
    }
}
