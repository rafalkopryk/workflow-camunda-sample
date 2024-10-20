public sealed class ZeebeResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string GprcEndpointName = "grpc";
    internal const string HttpEndpointName = "http";
    internal const string RestEndpointName = "rest";

    private EndpointReference? _restReference;

    public EndpointReference RestEndpoint =>
        _restReference ??= new(this, RestEndpointName);

    private EndpointReference? _grpcReference;
    public EndpointReference GrpcEndpoint =>
        _grpcReference ??= new(this, GprcEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{RestEndpoint.Property(EndpointProperty.Scheme)}://{RestEndpoint.Property(EndpointProperty.Host)}:{RestEndpoint.Property(EndpointProperty.Port)}/v2/"
        );
    public ReferenceExpression GrpcConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{GrpcEndpoint.Property(EndpointProperty.Scheme)}://{GrpcEndpoint.Property(EndpointProperty.Host)}:{GrpcEndpoint.Property(EndpointProperty.Port)}"
        );
}
