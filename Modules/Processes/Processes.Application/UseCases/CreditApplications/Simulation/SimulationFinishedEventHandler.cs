using Camunda.Client;
using MassTransit;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[ZeebeMessage(Name = "Message_SimulationFinished")]
[EntityName("event.credit.calculations.simulationFinished.v1")]
[MessageUrn("event.credit.calculations.simulationFinished.v1")]
public record SimulationFinished(string ApplicationId, string Decision);

internal class SimulationFinishedEventHandler(IMessageClient messageClient) : IConsumer<SimulationFinished>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<SimulationFinished> context)
    {
        await _client.Publish(context.Message.ApplicationId, context.Message);
    }
}
