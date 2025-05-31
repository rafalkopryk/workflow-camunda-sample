using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[ZeebeMessage(Name = "Message_ApplicationRegistered", TimeToLiveInMs = 24 * 3600 * 1000)]
[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome, string DocumentId);

[WolverineHandler]
public class ApplicationRegisteredEventHandler(IMessageClient messageClient)
{
    public async Task Handle(ApplicationRegistered message)
    {
        await messageClient.Publish(null, message, message.ApplicationId);
    }
}
