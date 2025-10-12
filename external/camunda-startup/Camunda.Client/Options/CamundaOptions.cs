namespace Camunda.Client.Options;

public record CamundaOptions
{
    public string Endpoint { get; set; }
    public string GrpcEndpoint { get; set; }
}