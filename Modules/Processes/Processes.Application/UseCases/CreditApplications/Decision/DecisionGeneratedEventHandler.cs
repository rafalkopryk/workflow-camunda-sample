using Camunda.Client;
using Common.Kafka;
using MassTransit;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[ZeebeMessage(Name = "Message_DecisionGenerated")]
[EntityName("event.credit.applications.decisionGenerated.v1")]
[MessageUrn("event.credit.applications.decisionGenerated.v1")]
public record DecisionGenerated(string ApplicationId) : INotification;

internal class DecisionGeneratedEventHandler(IMessageClient messageClient) : INotificationHandler<DecisionGenerated>, IConsumer<DecisionGenerated>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<DecisionGenerated> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(DecisionGenerated notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
