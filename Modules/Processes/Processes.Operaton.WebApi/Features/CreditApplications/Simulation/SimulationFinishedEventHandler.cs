using Processes.Operaton.WebApi.Operaton;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Simulation;

public sealed class SimulationFinishedEventHandler(OperatonClient client)
{
    public Task Handle(SimulationFinished message, CancellationToken cancellationToken) =>
        client.CorrelateMessageAsync("Message_SimulationFinished", message.ApplicationId, message, cancellationToken);
}
