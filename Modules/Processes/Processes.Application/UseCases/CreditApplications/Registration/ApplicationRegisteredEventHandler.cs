using Camunda.Client;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[ZeebeMessage(Name = "Message_ApplicationRegistered", TimeToLiveInMs = 24 * 3600 * 1000)]
[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

[WolverineHandler]
public class ApplicationRegisteredEventHandler(IMessageClient messageClient)
{
    private readonly IMessageClient _client = messageClient;

    public async Task Handle(ApplicationRegistered message)
    {
        await _client.Publish(null, message, message.ApplicationId);
    }
}
