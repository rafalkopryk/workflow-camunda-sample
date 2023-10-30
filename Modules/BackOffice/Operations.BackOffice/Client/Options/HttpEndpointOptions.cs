using System.Text.Json.Serialization;

public record HttpEndpointOptions
{
    [JsonPropertyName("url")]
    public string Url { get; init; }
}
