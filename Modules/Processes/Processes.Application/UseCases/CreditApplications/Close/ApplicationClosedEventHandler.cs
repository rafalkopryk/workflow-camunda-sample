using Camunda.Client;
using Common.Kafka;
using MassTransit;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Close;

[ZeebeMessage(Name = "Message_ApplicationClosed")]
[EntityName("event.credit.applications.applicationClosed.v1")]
[MessageUrn("event.credit.applications.applicationClosed.v1")]
public record ApplicationClosed(string ApplicationId) : INotification;

internal class ApplicationClosedEventHandler(IMessageClient messageClient) : INotificationHandler<ApplicationClosed>, IConsumer<ApplicationClosed>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<ApplicationClosed> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(ApplicationClosed notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
