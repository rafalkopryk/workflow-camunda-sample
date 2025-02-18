using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.EventHandlers.Close;

[ZeebeMessage(Name = "Message_ApplicationClosed")]
[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);

public class ApplicationClosedEventHandler(IMessageClient messageClient) 
{
    public async Task Handle(ApplicationClosed message)
    {
        await messageClient.Publish(message.ApplicationId, message);
    }
}
