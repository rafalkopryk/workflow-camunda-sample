using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, string Decision);

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
