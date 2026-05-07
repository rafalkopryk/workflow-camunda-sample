using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[MessageIdentity("simulationFinished", Version=1)]
public record SimulationFinished(string ApplicationId, string SimulationStatus);

public class SimulationFinishedEventHandler(CamundaClient camundaClient)
{
    public async Task Handle(SimulationFinished message)
    {
        await camundaClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_SimulationFinished",
            CorrelationKey = message.ApplicationId,
            Variables = message,
            MessageId = Guid.CreateVersion7().ToString(),
        });
    }
}
