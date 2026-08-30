using Calculations.Contracts;
using Elsa.Workflows.Runtime;
using Wolverine.Attributes;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Simulation;

[WolverineHandler]
public class SimulationFinishedEventHandler(IEventPublisher publisher)
{
    public async Task Handle(SimulationFinished message)
    {
        await publisher.PublishAsync(
            CreditApplicationEventNames.SimulationFinished,
            message.ApplicationId,
            payload: message);
    }
}