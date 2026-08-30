using Calculations.Contracts;
using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.CustomerVerification;

[WolverineHandler]
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
