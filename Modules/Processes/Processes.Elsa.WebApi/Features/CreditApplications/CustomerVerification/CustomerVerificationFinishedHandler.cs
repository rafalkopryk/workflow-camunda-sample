using Calculations.Contracts;
using Elsa.Workflows.Runtime;
using Wolverine.Attributes;

namespace Processes.Elsa.WebApi.Features.CreditApplications.CustomerVerification;

[WolverineHandler]
public class CustomerVerificationFinishedHandler(IEventPublisher publisher)
{
    public async Task Handle(CustomerVerified message)
    {
        await publisher.PublishAsync(
            CreditApplicationEventNames.CustomerVerified,
            message.ApplicationId,
            payload: message);
    }
}