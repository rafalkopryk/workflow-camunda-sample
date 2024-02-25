using Camunda.Client;
using Common.Kafka;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Contract;

[ZeebeMessage(Name = "Message_ContractSigned")]
[EventEnvelope(Topic = "event.credit.applications.contractSigned.v1")]
public record ContractSigned(string ApplicationId) : INotification;

internal class ContractSignedEventHandler(IMessageClient messageClient) : INotificationHandler<ContractSigned>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(ContractSigned notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
