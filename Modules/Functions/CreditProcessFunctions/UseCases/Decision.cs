using Azure.Messaging.ServiceBus;
using CreditProcessFunctions.Extensions;
using CreditProcessFunctions.UseCases.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CreditProcessFunctions.UseCases;

public class Decision(ILogger<Decision> logger, IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory)
{
    private readonly ILogger<Decision> _logger = logger;

    private readonly ServiceBusSender _sender = serviceBusSenderFactory.CreateClient(TopicAttributes.CLOSE);

    [Function(nameof(Decision))]
    public async Task Run(
        [ServiceBusTrigger(TopicAttributes.DECISIONGENERATED, TopicAttributes.DECISIONGENERATED_SUBSCRIPTION, Connection = "AzServiceBusConnectionString")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var body = JsonSerializer.Deserialize<DecisionGenerated>(message.Body, CustomJsonSerializerOptions.CamelCase);
        if (body.Decision == DecisionEnum.Negative)
        {
            await _sender.SendMessageAsync(new CloseApplicationCommand(body.ApplicationId));
        }

        await messageActions.CompleteMessageAsync(message);
    }
}


public record DecisionGenerated(string ApplicationId, DecisionEnum Decision);

public record CloseApplicationCommand(string ApplicationId);
