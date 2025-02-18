using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.EventHandlers.CustomerVerification;

[MessageIdentity("customerVerified", Version = 1)]
[ZeebeMessage(Name = "Message_CustomerVerified")]
public record CustomerVerified(string ApplicationId, string CustomerVerificationStatus);

public class CustomerVerificationFinishedHandler(IMessageClient messageClient)
{
    public async Task Handle(CustomerVerified message, CancellationToken ct)
    {
        await messageClient.Publish(message.ApplicationId, message);
    }
}