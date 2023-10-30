using System.Text.Json.Serialization;

public record ExternalServicesOptions
{
    [JsonPropertyName("operations")]
    public HttpEndpointOptions Operations { get;init;}
}
