public static class CamundaBuilderExtensions
{
    private const int DefaultGrpcPort = 26500;
    private const int DefaultRestPort = 8080;

    public static IResourceBuilder<ZeebeResource> AddZeebe(this IDistributedApplicationBuilder builder, string name, int? restPort = 8089)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var zeebeContainer = new ZeebeResource(name);

        return builder
            .AddResource(zeebeContainer)
            .WithHttpEndpoint(port: DefaultGrpcPort, targetPort: DefaultGrpcPort, ZeebeResource.GprcEndpointName)
            .WithHttpEndpoint(port: restPort, targetPort: DefaultRestPort, name: ZeebeResource.RestEndpointName)
            .WithHttpEndpoint(port: 9600, targetPort: 9600, name: "http")
            .WithImage("camunda/zeebe")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGECOMMANDWATERMARK", "0.998")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGEREPLICATIONWATERMARK", "0.999");
    }

    public static IResourceBuilder<T> WithZeebeReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<ZeebeResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("Camunda__CamundaRest__Endpoint", source.Resource.ConnectionStringExpression)
            .WithEnvironment("Camunda__CamundaGrpc__Endpoint", source.Resource.GrpcConnectionStringExpression)
            .WithEnvironment("Camunda__JobWorkers__Default__TimeoutInMs", "60000")
            .WithEnvironment("Camunda__JobWorkers__Default__PoolingMaxJobsToActivate", "5")
            .WithEnvironment("Camunda__JobWorkers__Default__PoolingRequestTimeoutInMs", "10000")
            .WithEnvironment("Camunda__JobWorkers__Default__PoolingDelayInMs", "200")
            .WithEnvironment("Camunda__JobWorkers__Default__UseStream", "true")
            .WithEnvironment("Camunda__JobWorkers__Default__StreamTimeoutInSec", "900");
    }
}
