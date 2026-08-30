using Calculations.Contracts;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.CustomerVerification;

[WolverineHandler]
public class CustomerVerificationFinishedHandler(ConductorWorkflowService workflows)
{
    public Task Handle(CustomerVerified message, CancellationToken cancellationToken)
    {
        return workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitCustomerVerificationTask,
            new Dictionary<string, object>
            {
                ["customerVerificationStatus"] = message.CustomerVerificationStatus.ToString(),
            });
    }
}
