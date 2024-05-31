using Camunda.Client;
using Common.Kafka;
using MassTransit;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Contract;

[ZeebeMessage(Name = "Message_ContractSigned")]
[EntityName("event.credit.applications.contractSigned.v1")]
[MessageUrn("event.credit.applications.contractSigned.v1")]
public record ContractSigned(string ApplicationId) : INotification;

internal class ContractSignedEventHandler(IMessageClient messageClient) : INotificationHandler<ContractSigned>, IConsumer<ContractSigned>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<ContractSigned> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(ContractSigned notification, CancellationToken cancellationToken)
    {
        await _client.Publish(notification.ApplicationId, notification);
    }
}
