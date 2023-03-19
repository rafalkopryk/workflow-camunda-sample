using System.Text.Json;
using System.Text.Json.Serialization;

namespace Camunda.Connector.Kafka;

internal static class JsonSerializerKafkaOptions
{
    public static readonly JsonSerializerOptions CamelCase = GetJsonSerializerOptions();

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}