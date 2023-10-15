using Newtonsoft.Json;

public record ExternalServicesOptions
{
    [JsonProperty("applications")]
    public HttpEndpointOptions Applications { get;init;}
}
