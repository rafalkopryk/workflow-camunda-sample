using Camunda.Client;
using Common.Kafka;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[ZeebeMessage(Name = "Message_DecisionGenerated")]
[EventEnvelope(Topic = "event.credit.applications.decisionGenerated.v1")]
public record DecisionGenerated(string ApplicationId) : INotification;

internal class DecisionGeneratedEventHandler(IMessageClient messageClient) : INotificationHandler<DecisionGenerated>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(DecisionGenerated notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
