using Camunda.Client;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Close;

[ZeebeMessage(Name = "Message_ApplicationClosed")]
[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);

public class ApplicationClosedEventHandler(IMessageClient messageClient) 
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(ApplicationClosed message)
    {
        await _client.Publish(message.ApplicationId, message);
    }
}
