using Newtonsoft.Json;

public record HttpEndpointOptions
{
    [JsonProperty("url")]
    public string Url { get; init; }
}
