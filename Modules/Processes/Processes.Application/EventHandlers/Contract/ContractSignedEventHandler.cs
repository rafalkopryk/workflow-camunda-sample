using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.EventHandlers.Contract;

[ZeebeMessage(Name = "Message_ContractSigned")]
[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

public class ContractSignedEventHandler(IMessageClient messageClient) 
{
    public async Task Handle(ContractSigned message)
    {
        await messageClient.Publish(message.ApplicationId, message);
    }
}
