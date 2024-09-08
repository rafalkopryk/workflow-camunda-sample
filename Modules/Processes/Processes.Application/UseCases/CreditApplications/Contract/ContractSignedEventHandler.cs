using Camunda.Client;
using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Contract;

[ZeebeMessage(Name = "Message_ContractSigned")]
[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

public class ContractSignedEventHandler(IMessageClient messageClient) 
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(ContractSigned message)
    {
        await _client.Publish(message.ApplicationId, message);
    }
}
