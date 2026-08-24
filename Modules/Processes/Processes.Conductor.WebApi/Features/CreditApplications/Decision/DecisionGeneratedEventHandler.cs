using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Decision;

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, string Decision);

[WolverineHandler]
public class DecisionGeneratedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(DecisionGenerated message, CancellationToken cancellationToken) =>
        workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitDecisionTask,
            new Dictionary<string, object> { ["decision"] = message.Decision });
}
