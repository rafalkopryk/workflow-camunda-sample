using Camunda.Client.Messages;
using Camunda.Client.Rest;
using System.Text.Json;

namespace Camunda.Client;

internal class RestMessageClient(ICamundaClientRest client) : IMessageClient
{
    public async Task Publish<T>(string correlationKey, T message, string? messageId = null)
    {
        try
        {
            var attribute = message.GetType().GetAttribute<ZeebeMessageAttribute>();
            ArgumentNullException.ThrowIfNull(attribute);

            var json = JsonSerializer.Serialize(message, JsonSerializerCustomOptions.CamelCase);
            var jsonAsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            await client.PublicationAsync(new MessagePublicationRequest
            {
                Name = attribute!.Name,
                CorrelationKey = correlationKey ?? string.Empty,
                Variables = jsonAsObject,
                MessageId = messageId ?? string.Empty,
                TimeToLive = attribute.TimeToLiveInMs,
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
