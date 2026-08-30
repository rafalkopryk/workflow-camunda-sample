using Applications.Contracts.Events;
using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Close;

[WolverineHandler]
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
