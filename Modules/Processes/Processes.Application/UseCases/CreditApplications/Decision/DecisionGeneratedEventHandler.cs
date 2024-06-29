using Camunda.Client;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[ZeebeMessage(Name = "Message_DecisionGenerated")]
[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, string Decision);

[WolverineHandler]
public class DecisionGeneratedEventHandler(IMessageClient messageClient)
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(DecisionGenerated message)
    {
        await _client.Publish(message.ApplicationId, message);
    }
}
