using Camunda.Client.Messages;
using Wolverine.Attributes;

namespace Processes.Application.EventHandlers.Registration;

//[ZeebeMessage(Name = "Message_ApplicationRegistered", TimeToLiveInMs = 24 * 3600 * 1000)]
[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome, string Pesel, string ProcessCode);

[WolverineHandler]
public class ApplicationRegisteredEventHandler(IMessageClient messageClient)
{
    public async Task Handle(ApplicationRegistered message)
    {
        var messageName = message.ProcessCode switch
        {
            "Fast" => "Message_FastApplicationRegistered",
            _ => "Message_ApplicationRegistered"
        };

        await messageClient.Publish(messageName, null, message, 24 * 3600 * 1000, message.ApplicationId);
    }
}