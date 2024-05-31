using Azure.Messaging.ServiceBus;
using CreditProcessFunctions.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditProcessFunctions.UseCases;

public class Registration(ILogger<Registration> logger, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory)
{
    private readonly ILogger<Registration> _logger = logger;

    private readonly ServiceBusSender _sender = serviceBusSenderFactory.CreateClient(TopicAttributes.SIMULATION);

    [Function(nameof(Registration))]
    public async Task Run(
        [ServiceBusTrigger(TopicAttributes.APPLICATIONREGISTERED, TopicAttributes.APPLICATIONREGISTERED_SUBSCRIPTION, Connection = "AzServiceBusConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var body = JsonSerializer.Deserialize<ApplicationRegistered>(message.Body, CustomJsonSerializerOptions.CamelCase);

        await _sender.SendMessageAsync(new SimulationCommand(
            body.ApplicationId,
            body.Amount,
            body.CreditPeriodInMonths,
            body.AverageNetMonthlyIncome));

        await messageActions.CompleteMessageAsync(message);
    }
}

public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

