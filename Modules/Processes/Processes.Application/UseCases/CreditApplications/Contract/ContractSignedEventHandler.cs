using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Contract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

public class ContractSignedEventHandler(CamundaClient camundaClient)
{
    public async Task Handle(ContractSigned message)
    {
        await camundaClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_ContractSigned",
            CorrelationKey = message.ApplicationId,
            Variables = message,
            MessageId = Guid.CreateVersion7().ToString(),
        });
    }
}
