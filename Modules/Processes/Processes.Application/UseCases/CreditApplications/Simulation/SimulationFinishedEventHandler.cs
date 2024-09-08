using Camunda.Client;
using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[ZeebeMessage(Name = "Message_SimulationFinished")]
[MessageIdentity("simulationFinished", Version=1)]
public record SimulationFinished(string ApplicationId, string Decision);

public class SimulationFinishedEventHandler(IMessageClient messageClient)
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(SimulationFinished message)
    {
        await _client.Publish(message.ApplicationId, message);
    }
}
