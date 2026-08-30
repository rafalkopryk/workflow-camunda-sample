using Applications.Contracts.Events;
using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Contract;

[WolverineHandler]
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
