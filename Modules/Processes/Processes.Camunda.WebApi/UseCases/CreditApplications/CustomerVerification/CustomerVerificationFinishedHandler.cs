using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, string CustomerVerificationStatus);

public class CustomerVerificationFinishedHandler(CamundaClient camundaClient)
{
    public async Task Handle(CustomerVerified message, CancellationToken ct)
    {
        await camundaClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_CustomerVerified",
            CorrelationKey = message.ApplicationId,
            Variables = message,
            MessageId = Guid.CreateVersion7().ToString(),
        });
    }
}
