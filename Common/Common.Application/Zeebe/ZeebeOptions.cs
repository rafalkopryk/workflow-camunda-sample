using System.Text.Json.Serialization;

namespace Common.Application.Zeebe;

public record ZeebeOptions
{
    [JsonPropertyName("CLOUD")]
    public bool Cloud { get; init; }

    [JsonPropertyName("ADDRESS")]
    public string Address { get; init; }

    [JsonPropertyName("CLIENT")]
    public ZeebeOptionsClient Client { get; init; }
}