using Calculations.Contracts;
using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.Features.CreditApplications.CustomerVerification;

[WolverineHandler]
public class CustomerVerificationFinishedHandler(ITemporalClient messageClient)
{
    public async Task Handle(CustomerVerified message, CancellationToken ct)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnCustomerVerificationCompletedAsync(message.CustomerVerificationStatus));
    }
}