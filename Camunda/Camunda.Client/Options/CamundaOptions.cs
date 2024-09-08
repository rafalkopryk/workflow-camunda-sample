namespace Camunda.Client.Options;

public record CamundaOptions
{
    public bool UseRest { get; init; }
    public GrpcCamundaOptions? CamundaGrpc { get; init; }
    public RestCamundaOptions? CamundaRest { get; init; }
}

public record GrpcCamundaOptions
{
    public string Endpoint { get; init; }
}

public record RestCamundaOptions
{
    public string Endpoint { get; init; }
}