using Camunda.Client;
using MassTransit;

namespace Processes.Application.UseCases.CreditApplications.Contract;

[ZeebeMessage(Name = "Message_ContractSigned")]
[EntityName("event.credit.applications.contractSigned.v1")]
[MessageUrn("event.credit.applications.contractSigned.v1")]
public record ContractSigned(string ApplicationId);

internal class ContractSignedEventHandler(IMessageClient messageClient) :  IConsumer<ContractSigned>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<ContractSigned> context)
    {
        await _client.Publish(context.Message.ApplicationId, context.Message);
    }
}
