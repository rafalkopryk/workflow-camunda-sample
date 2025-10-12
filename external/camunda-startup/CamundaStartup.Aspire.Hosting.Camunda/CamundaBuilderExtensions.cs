using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace CamundaStartup.Aspire.Hosting.Camunda;

public static class CamundaBuilderExtensions
{
    private const int DefaultGrpcPort = 26500;
    private const int DefaultRestPort = 8080;

    public static IResourceBuilder<CamundaResource> AddCamunda(this IDistributedApplicationBuilder builder, [ResourceName] string name, int? port, ReferenceExpression? elasticConnectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var zeebeContainer = new CamundaResource(name);
        return builder
            .AddResource(zeebeContainer)
            .WithHttpEndpoint(port: port, targetPort: DefaultRestPort, name: CamundaResource.RestEndpointName)
            .WithHttpEndpoint(port: DefaultGrpcPort, targetPort: DefaultGrpcPort, CamundaResource.GprcEndpointName)
            .WithHttpEndpoint(port: 9600, targetPort: 9600, name: "internal")
            .WithImage(CamundaContainerImageTags.Image, CamundaContainerImageTags.Tag)
            
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_METHOD", "basic")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_UNPROTECTEDAPI", "true")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHORIZATIONS_ENABLED", "false")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_USERNAME", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_PASSWORD", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_NAME", "Demo User")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_EMAIL", "demo@demo.com")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_DEFAULTROLES_ADMIN_USERS[0]", "demo")
            
            .WithElasticDatabase(elasticConnectionString)
            
            .WithHttpHealthCheck("actuator/health/readiness", 200, "internal");
    }

    private static IResourceBuilder<CamundaResource> WithElasticDatabase(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString)
    {
        ArgumentNullException.ThrowIfNull(elasticConnectionString);

        builder.Resource.CamundaDatabaseConnectionStringExpression = elasticConnectionString;

        builder.WithEnvironment("CAMUNDA_DATABASE_INDEX_NUMBEROFREPLICAS", "0");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_TYPE", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_ELASTICSEARCH_CLUSTERNAME", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_ELASTICSEARCH_URL", builder.Resource.CamundaDatabaseConnectionStringExpression);
        
        return builder;
    }
    
    public static IResourceBuilder<CamundaResource> WithDataVolume(this IResourceBuilder<CamundaResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name, "/usr/local/zeebe/data", isReadOnly);
    }
}
