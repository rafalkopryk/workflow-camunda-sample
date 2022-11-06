using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Zeebe;

internal static class JsonSerializerCustomOptions
{
    public static readonly JsonSerializerOptions CamelCase = GetJsonSerializerOptions();

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}