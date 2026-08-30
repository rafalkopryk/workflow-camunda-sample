using Applications.Contracts.Events;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Decision;

[WolverineHandler]
public class DecisionGeneratedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(DecisionGenerated message, CancellationToken cancellationToken)
    {
        return workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitDecisionTask,
            new Dictionary<string, object> { ["decision"] = message.Decision.ToString() });
    }
}
