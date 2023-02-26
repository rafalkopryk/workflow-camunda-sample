using System.Text.Json.Serialization;

namespace Camunda.Client;

public record ZeebeOptions
{
    [JsonPropertyName("ADDRESS")]
    public string Address { get; init; }
}