using Applications.Contracts.Events;
using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Registration;

[WolverineHandler]
public class ApplicationRegisteredEventHandler(CamundaClient camundaClient)
{
    public async Task Handle(ApplicationRegistered message)
    {
        await camundaClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_ApplicationRegistered",
            Variables = message,
            MessageId = message.ApplicationId,
            TimeToLive = 24 * 3600 * 1000,
        });
    }
}
