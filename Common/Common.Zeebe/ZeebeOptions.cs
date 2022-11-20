using System.Text.Json.Serialization;

namespace Common.Application.Zeebe;

public record ZeebeOptions
{
    [JsonPropertyName("ADDRESS")]
    public string Address { get; init; }
}