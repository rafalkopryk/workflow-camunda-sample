using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[CamundaMessage(Name = "Message_SimulationFinished")]
[MessageIdentity("simulationFinished", Version=1)]
public record SimulationFinished(string ApplicationId, string SimulationStatus);

public class SimulationFinishedEventHandler(IMessageClient messageClient)
{
    public async Task Handle(SimulationFinished message)
    {
        await messageClient.Publish(message.ApplicationId, message);
    }
}
