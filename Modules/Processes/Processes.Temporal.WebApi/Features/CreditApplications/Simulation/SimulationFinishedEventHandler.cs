using Calculations.Contracts;
using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Simulation;

[WolverineHandler]
public class SimulationFinishedEventHandler(ITemporalClient messageClient)
{
    public async Task Handle(SimulationFinished message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnSimulationCompletedAsync(message.SimulationStatus));
    }
}
