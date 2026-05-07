using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Close;

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);

public class ApplicationClosedEventHandler(CamundaClient camundaClient)
{
    public async Task Handle(ApplicationClosed message)
    {
        await camundaClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_ApplicationClosed",
            CorrelationKey = message.ApplicationId,
            Variables = message,
            MessageId = Guid.CreateVersion7().ToString(),
        });
    }
}
