using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.EventHandlers.Decision;

[ZeebeMessage(Name = "Message_DecisionGenerated")]
[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, string Decision);

[WolverineHandler]
public class DecisionGeneratedEventHandler(IMessageClient messageClient)
{
    public async Task Handle(DecisionGenerated message)
    {
        await messageClient.Publish(message.ApplicationId, message);
    }
}
