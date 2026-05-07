using Camunda.Orchestration.Sdk;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome, string DocumentId);

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
