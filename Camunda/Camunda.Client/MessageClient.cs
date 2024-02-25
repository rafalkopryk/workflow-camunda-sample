using GatewayProtocol;
using System.Text.Json;

namespace Camunda.Client;

internal class MessageClient(Gateway.GatewayClient client) : IMessageClient
{
    private readonly Gateway.GatewayClient _client = client;

    public async Task Publish<T>(string correlationKey, T message, string? messageId = null)
    {
        var attribute = message.GetType().GetAttribute<ZeebeMessageAttribute>();
        if (attribute is null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        await _client.PublishMessageAsync(new PublishMessageRequest
        {
            Name = attribute!.Name,
            CorrelationKey = correlationKey ?? string.Empty,
            MessageId = messageId ?? string.Empty,
            Variables = JsonSerializer.Serialize(message, JsonSerializerCustomOptions.CamelCase),
            TimeToLive = attribute!.TimeToLiveInMs,
        });
    }
}
