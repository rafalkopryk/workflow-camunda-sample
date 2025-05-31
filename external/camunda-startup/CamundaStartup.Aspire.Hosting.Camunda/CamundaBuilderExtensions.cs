using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace CamundaStartup.Aspire.Hosting.Camunda;

public static class CamundaBuilderExtensions
{
    private const int DefaultGrpcPort = 26500;
    private const int DefaultRestPort = 8080;

    public static IResourceBuilder<CamundaResource> AddCamunda(this IDistributedApplicationBuilder builder, [ResourceName] string name, int? port)
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
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGECOMMANDWATERMARK", "0.998")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGEREPLICATIONWATERMARK", "0.999")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_MODE", "none")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_UNPROTECTEDAPI", "true")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHORIZATIONS_ENABLED", "false")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_EMAIL", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_NAME", "Demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_PASSWORD", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_EMAIL", "demo@demo.com")
            .WithEnvironment("SPRING_PROFILES_ACTIVE", "broker,consolidated-auth")
            .WithHttpHealthCheck("actuator/health/readiness", 200, "internal");
    }

    public static IResourceBuilder<CamundaResource> WithElasticDatabase(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString)
    {
        ArgumentNullException.ThrowIfNull(elasticConnectionString);

        builder.Resource.CamundaDatabaseConnectionStringExpression = elasticConnectionString;

        builder.WithEnvironment("CAMUNDA_DATABASE_TYPE", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATABASE_CLUSTERNAME", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATABASE_URL", builder.Resource.CamundaDatabaseConnectionStringExpression);

        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_CLASSNAME", "io.camunda.exporter.CamundaExporter");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_CONNECT_URL", elasticConnectionString);
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_CONNECT_TYPE", "elasticsearch");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_BULK_SIZE", "1000");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_BULK_DELAY", "1");

        return builder;
    }

    public static IResourceBuilder<CamundaResource> WithOperate(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString = null)
    {
        elasticConnectionString ??= builder.Resource.CamundaDatabaseConnectionStringExpression;

        ArgumentNullException.ThrowIfNull(elasticConnectionString);

        var zeebeConnectionString = ReferenceExpression.Create($"{builder.Resource.GrpcEndpoint.Property(EndpointProperty.Host)}:{builder.Resource.GrpcEndpoint.Property(EndpointProperty.Port)}");

        builder
            .WithEnvironment("CAMUNDA_OPERATE_ZEEBE_GATEWAYADDRESS", zeebeConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_ELASTICSEARCH_URL", elasticConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_ZEEBEELASTICSEARCH_URL", elasticConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_IMPORTER_ENABLED", "false")
            .WithEnvironment("SPRING_PROFILES_ACTIVE", "operate,broker,consolidated-auth");

        return builder;
    }

    public static IResourceBuilder<CamundaResource> WithDataVolume(this IResourceBuilder<CamundaResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name, "/usr/local/zeebe/data", isReadOnly);
    }
}
