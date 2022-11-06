using System.Text.Json.Serialization;

namespace Common.Application.Zeebe;

public record ZeebeOptionsClient
{
    [JsonPropertyName("ID")]
    public string Id { get; init; }

    [JsonPropertyName("SECRET")]
    public string Secret { get; init; }
}



