using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Simulation;

[MessageIdentity("simulationFinished", Version=1)]
public record SimulationFinished(string ApplicationId, string SimulationStatus);

public class SimulationFinishedEventHandler(ITemporalClient messageClient)
{
    public async Task Handle(SimulationFinished message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnSimulationCompletedAsync(message.SimulationStatus));
    }
}
