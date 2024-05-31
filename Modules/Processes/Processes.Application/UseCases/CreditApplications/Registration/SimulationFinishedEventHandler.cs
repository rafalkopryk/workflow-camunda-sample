using Camunda.Client;
using Common.Kafka;
using MassTransit;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Simulation;


[ZeebeMessage(Name = "Message_ApplicationRegistered", TimeToLiveInMs = 24 * 3600 * 1000)]
[EntityName("event.credit.applications.applicationRegistered.v1")]
[MessageUrn("event.credit.applications.applicationRegistered.v1")]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome) : INotification;

internal class ApplicationRegisteredEventHandler(IMessageClient messageClient) : INotificationHandler<ApplicationRegistered>, IConsumer<ApplicationRegistered>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<ApplicationRegistered> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(ApplicationRegistered notification, CancellationToken cancellationToken)
    {
        await _client.Publish(
            null,
            notification,
            notification.ApplicationId);
    }
}
