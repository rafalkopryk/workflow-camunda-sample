using Camunda.Client;
using Common.Kafka;
using MassTransit;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[ZeebeMessage(Name = "Message_SimulationFinished")]
[EntityName("event.credit.calculations.simulationFinished.v1")]
[MessageUrn("event.credit.calculations.simulationFinished.v1")]
public record SimulationFinished(string ApplicationId, string Decision) : INotification;

internal class SimulationFinishedEventHandler(IMessageClient messageClient) : INotificationHandler<SimulationFinished>, IConsumer<SimulationFinished>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<SimulationFinished> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(SimulationFinished notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
