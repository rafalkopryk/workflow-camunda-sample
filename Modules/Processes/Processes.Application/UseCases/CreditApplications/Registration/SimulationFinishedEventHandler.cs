using Camunda.Client;
using MassTransit;

namespace Processes.Application.UseCases.CreditApplications.Simulation;


[ZeebeMessage(Name = "Message_ApplicationRegistered", TimeToLiveInMs = 24 * 3600 * 1000)]
[EntityName("event.credit.applications.applicationRegistered.v1")]
[MessageUrn("event.credit.applications.applicationRegistered.v1")]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

internal class ApplicationRegisteredEventHandler(IMessageClient messageClient) : IConsumer<ApplicationRegistered>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<ApplicationRegistered> context)
    {
        await _client.Publish(null, context.Message, context.Message.ApplicationId);

    }
}
