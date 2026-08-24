using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, string CustomerVerificationStatus);

public class CustomerVerificationFinishedHandler(ConductorWorkflowService workflows)
{
    public Task Handle(CustomerVerified message, CancellationToken cancellationToken)
    {
        return workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitCustomerVerificationTask,
            new Dictionary<string, object>
            {
                ["customerVerificationStatus"] = message.CustomerVerificationStatus,
            });
    }
}
