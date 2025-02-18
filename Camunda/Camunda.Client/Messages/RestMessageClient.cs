using Camunda.Client.Messages;
using Camunda.Client.Rest;
using System.Text.Json;
namespace Camunda.Client;

internal class RestMessageClient(ICamundaClientRest client) : IMessageClient
{
    public async Task Publish<T>(string correlationKey, T message, string? messageId = null)
    {
        var attribute = message.GetType().GetAttribute<ZeebeMessageAttribute>();
        ArgumentNullException.ThrowIfNull(attribute);

        await Publish(attribute!.Name, correlationKey, message, attribute.TimeToLiveInMs, messageId);
    }

    public async Task Publish<T>(string name, string correlationKey, T message, long timeToLiveInMs, string? messageId = null)
    {
        var json = JsonSerializer.Serialize(message, JsonSerializerCustomOptions.CamelCase);
        var jsonAsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        await client.PublicationAsync(new MessagePublicationRequest
        {
            Name = name,
            CorrelationKey = correlationKey ?? string.Empty,
            Variables = jsonAsObject,
            MessageId = messageId ?? string.Empty,
            TimeToLive = timeToLiveInMs,
        });
    }
}
