using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class CamundaBuilderExtensions
{
    private const int DefaultGrpcPort = 26500;
    private const int DefaultRestPort = 8080;
    
    public static IResourceBuilder<ZeebeResource> WithOperate(this IResourceBuilder<ZeebeResource> builder, string name, ReferenceExpression elasticConnectionString , int port = 8085)
    {
        var zeebeConnectionString = ReferenceExpression.Create($"{builder.Resource.GrpcEndpoint.Property(EndpointProperty.Host)}:{builder.Resource.GrpcEndpoint.Property(EndpointProperty.Port)}");

        var operateContainer = new ContainerResource(name);
        var resource = builder.ApplicationBuilder
            .AddResource(operateContainer)
            .WithHttpEndpoint(port: port, targetPort: 8080, name: "http")
            .WithImage("camunda/operate", "8.6.3")
            .WithEnvironment("CAMUNDA_OPERATE_ZEEBE_GATEWAYADDRESS", zeebeConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_ELASTICSEARCH_URL", elasticConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_ZEEBEELASTICSEARCH_URL", elasticConnectionString)
            //.WithHttpHealthCheck("actuator/health/readiness", 200, "http")
            .WaitFor(builder);

        return builder;
    }

    public static IResourceBuilder<ZeebeResource> AddZeebe(this IDistributedApplicationBuilder builder, string name, ReferenceExpression? elasticConnectionString ,int? restPort = 8089)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var zeebeContainer = new ZeebeResource(name);
        var resource = builder
            .AddResource(zeebeContainer)
            .WithHttpEndpoint(port: DefaultGrpcPort, targetPort: DefaultGrpcPort, ZeebeResource.GprcEndpointName)
            .WithHttpEndpoint(port: restPort, targetPort: DefaultRestPort, name: ZeebeResource.RestEndpointName)
            .WithHttpEndpoint(port: 9600, targetPort: 9600, name: "internal")
            .WithImage("camunda/zeebe", "8.6.3")
            .WithEnvironment("CAMUNDA_REST_QUERY_ENABLED", "true")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGECOMMANDWATERMARK", "0.998")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGEREPLICATIONWATERMARK", "0.999");
        
        if (elasticConnectionString != null)
        {   
            resource.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_CLASSNAME", "io.camunda.zeebe.exporter.ElasticsearchExporter");
            resource.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_ARGS_URL", elasticConnectionString);
            resource.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_ARGS_BULK_SIZE", "1000");
            resource.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_ARGS_BULK_DELAY", "1");
        }

        resource.WithHttpHealthCheck("actuator/health/readiness", 200, "internal");
        return resource;
    }
    
    public static IResourceBuilder<ZeebeResource> WithDataVolume(this IResourceBuilder<ZeebeResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name, "/usr/local/zeebe/data", isReadOnly);
    }

    public static IResourceBuilder<T> WithZeebeReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<ZeebeResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("Camunda__CamundaRest__Endpoint", source.Resource.ConnectionStringExpression)
            .WithEnvironment("Camunda__CamundaGrpc__Endpoint", source.Resource.GrpcConnectionStringExpression)
            .WithEnvironment("Camunda__JobWorkers__Default__TimeoutInMs", "30000")
            .WithEnvironment("Camunda__JobWorkers__Default__PoolingMaxJobsToActivate", "5")
            .WithEnvironment("Camunda__JobWorkers__Default__PoolingRequestTimeoutInMs", "-1")
            .WithEnvironment("Camunda__JobWorkers__Default__PoolingDelayInMs", "10000")
            .WithEnvironment("Camunda__JobWorkers__Default__UseStream", "true")
            .WithEnvironment("Camunda__JobWorkers__Default__StreamTimeoutInSec", "900");
    }
}

public sealed class ZeebeResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string GprcEndpointName = "grpc";
    internal const string HttpEndpointName = "http";
    internal const string RestEndpointName = "rest";

    private EndpointReference? _restReference;
    public EndpointReference RestEndpoint => _restReference ??= new(this, RestEndpointName);

    private EndpointReference? _grpcReference;
    public EndpointReference GrpcEndpoint => _grpcReference ??= new(this, GprcEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{RestEndpoint.Property(EndpointProperty.Scheme)}://{RestEndpoint.Property(EndpointProperty.Host)}:{RestEndpoint.Property(EndpointProperty.Port)}/v2/"
        );

    public ReferenceExpression GrpcConnectionStringExpression =>
        ReferenceExpression.Create(
            $"{GrpcEndpoint.Property(EndpointProperty.Scheme)}://{GrpcEndpoint.Property(EndpointProperty.Host)}:{GrpcEndpoint.Property(EndpointProperty.Port)}"
        );
}