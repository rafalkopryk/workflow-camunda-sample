using Camunda.Client;
using Common.Kafka;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Close;

[ZeebeMessage(Name = "Message_ApplicationClosed")]
[EventEnvelope(Topic = "event.credit.applications.applicationClosed.v1")]
public record ApplicationClosed(string ApplicationId) : INotification;

internal class ApplicationClosedEventHandler(IMessageClient messageClient) : INotificationHandler<ApplicationClosed>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(ApplicationClosed notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
