using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, string CustomerVerificationStatus);

public class CustomerVerificationFinishedHandler(ITemporalClient messageClient)
{
    public async Task Handle(CustomerVerified message, CancellationToken ct)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnCustomerVerificationCompletedAsync(message.CustomerVerificationStatus));
    }
}