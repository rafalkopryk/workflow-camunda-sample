using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Simulation;

[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationFinished(string ApplicationId, string SimulationStatus);

public class SimulationFinishedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(SimulationFinished message, CancellationToken cancellationToken) =>
        workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitSimulationTask,
            new Dictionary<string, object> { ["simulationStatus"] = message.SimulationStatus });
}
