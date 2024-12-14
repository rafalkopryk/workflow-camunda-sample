using System.Text.Json.Serialization;
using System.Text.Json;

public static class CustomJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}