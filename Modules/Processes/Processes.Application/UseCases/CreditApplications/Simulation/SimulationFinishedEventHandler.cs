using Camunda.Client;
using Common.Kafka;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[ZeebeMessage(Name = "Message_SimulationFinished")]
[EventEnvelope(Topic = "event.credit.calculations.simulationFinished.v1")]
public record SimulationFinished(string ApplicationId, string Decision) : INotification;

internal class SimulationFinishedEventHandler(IMessageClient messageClient) : INotificationHandler<SimulationFinished>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(SimulationFinished notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
