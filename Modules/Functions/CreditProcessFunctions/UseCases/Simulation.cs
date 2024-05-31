using Azure.Messaging.ServiceBus;
using CreditProcessFunctions.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditProcessFunctions.UseCases;

public class Simulation(ILogger<Simulation> logger, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory)
{
    private readonly ILogger<Simulation> _logger = logger;

    private readonly ServiceBusSender _sender = serviceBusSenderFactory.CreateClient(TopicAttributes.DECISION);

    [Function(nameof(Simulation))]
    public async Task Run(
        [ServiceBusTrigger(TopicAttributes.SIMULATIONFINISHED, TopicAttributes.SIMULATIONFINISHED_SUBSCRIPTION, Connection = "AzServiceBusConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var body = JsonSerializer.Deserialize<SimulationFinished>(message.Body, CustomJsonSerializerOptions.CamelCase);

        await _sender.SendMessageAsync(new DecisionCommand(body.ApplicationId, body.Decision));

        await messageActions.CompleteMessageAsync(message);
    }
}

public record SimulationFinished(string ApplicationId, string Decision);

public record DecisionCommand(string ApplicationId, string Decision);
