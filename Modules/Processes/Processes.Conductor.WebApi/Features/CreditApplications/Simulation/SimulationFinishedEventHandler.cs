using Calculations.Contracts;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Simulation;

[WolverineHandler]
public class SimulationFinishedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(SimulationFinished message, CancellationToken cancellationToken)
    {
        return workflows.CompleteWaitingTaskAsync(
            message.ApplicationId,
            CreditApplicationConductorNames.WaitSimulationTask,
            new Dictionary<string, object> { ["simulationStatus"] = message.SimulationStatus.ToString() });
    }
}
