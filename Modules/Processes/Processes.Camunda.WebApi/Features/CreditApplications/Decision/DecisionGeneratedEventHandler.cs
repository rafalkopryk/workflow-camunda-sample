using Applications.Contracts.Events;
using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Decision;

[WolverineHandler]
public class DecisionGeneratedEventHandler(CamundaClient camundaClient)
{
    public async Task Handle(DecisionGenerated message)
    {
        await camundaClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_DecisionGenerated",
            CorrelationKey = message.ApplicationId,
            Variables = message,
            MessageId = Guid.CreateVersion7().ToString(),
        });
    }
}
