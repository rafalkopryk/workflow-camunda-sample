using Applications.Contracts.Events;
using Elsa.Workflows.Runtime;
using Wolverine.Attributes;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Decision;

[WolverineHandler]
public class DecisionGeneratedEventHandler(IEventPublisher publisher)
{
    public async Task Handle(DecisionGenerated message)
    {
        await publisher.PublishAsync(
            CreditApplicationEventNames.DecisionGenerated,
            message.ApplicationId,
            payload: message);
    }
}
