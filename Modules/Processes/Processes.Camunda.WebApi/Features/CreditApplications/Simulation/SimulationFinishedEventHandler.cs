using Calculations.Contracts;
using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Simulation;

[WolverineHandler]
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
