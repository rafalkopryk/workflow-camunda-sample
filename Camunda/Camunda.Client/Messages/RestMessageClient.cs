﻿using Camunda.Client.Messages;
using Camunda.Client.Rest;
using System.Text.Json;

namespace Camunda.Client;

internal class RestMessageClient(ICamundaClientRest client) : IMessageClient
{
    private readonly ICamundaClientRest _client = client;

    public async Task Publish<T>(string correlationKey, T message, string? messageId = null)
    {
        try
        {
            var attribute = message.GetType().GetAttribute<ZeebeMessageAttribute>();
            ArgumentNullException.ThrowIfNull(attribute);

            var json = JsonSerializer.Serialize(message, JsonSerializerCustomOptions.CamelCase);
            var jsonAsObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            await _client.CorrelationAsync(new MessageCorrelationRequest
            {
                Name = attribute!.Name,
                CorrelationKey = correlationKey ?? messageId ?? string.Empty,
                Variables = jsonAsObject,
            });
        }
        catch (Exception ex)
        {

            throw;
        }
    }
}
