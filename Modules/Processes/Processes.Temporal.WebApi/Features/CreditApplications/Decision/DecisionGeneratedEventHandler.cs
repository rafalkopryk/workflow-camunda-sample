using Applications.Contracts.Events;
using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Decision;

[WolverineHandler]
public class DecisionGeneratedEventHandler(ITemporalClient messageClient)
{
    public async Task Handle(DecisionGenerated message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnDecisionCompletedAsync(message.Decision));
    }
}
