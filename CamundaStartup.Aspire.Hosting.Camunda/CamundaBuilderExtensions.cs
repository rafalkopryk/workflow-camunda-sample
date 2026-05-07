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
            
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_METHOD", "basic")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_UNPROTECTEDAPI", "true")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHORIZATIONS_ENABLED", "false")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_USERNAME", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_PASSWORD", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_NAME", "Demo User")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_EMAIL", "demo@demo.com")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_DEFAULTROLES_ADMIN_USERS[0]", "demo")
            
            .WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_TYPE", "none")
            .WithHttpHealthCheck("actuator/health/readiness", 200, "internal");
    }

    public static IResourceBuilder<CamundaResource> WithElasticDatabase(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString)
    {
        ArgumentNullException.ThrowIfNull(elasticConnectionString);

        builder.Resource.CamundaDatabaseConnectionStringExpression = elasticConnectionString;

        builder.WithEnvironment("CAMUNDA_DATABASE_INDEX_NUMBEROFREPLICAS", "0");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_TYPE", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_ELASTICSEARCH_CLUSTERNAME", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_ELASTICSEARCH_URL", builder.Resource.CamundaDatabaseConnectionStringExpression);
        
        return builder;
    }
    
    public static IResourceBuilder<CamundaResource> WithRdmbsDatabase(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? jdbcConnectionString, ReferenceExpression user, ParameterResource password)
    {
        builder.WithEnvironment("CAMUNDA_DATABASE_INDEX_NUMBEROFREPLICAS", "0");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_TYPE", "rdbms");
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_RDBMS_URL", jdbcConnectionString);
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_RDBMS_USERNAME", user);
        builder.WithEnvironment("CAMUNDA_DATA_SECONDARYSTORAGE_RDBMS_PASSWORD", password);
        
        return builder;
    }
    
    public static IResourceBuilder<CamundaResource> WithDataVolume(this IResourceBuilder<CamundaResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .WithVolume(name, "/usr/local/camunda/data", isReadOnly);
    }
    
    public static IResourceBuilder<CamundaResource> WithS3Backup(
        this IResourceBuilder<CamundaResource> builder,
        ReferenceExpression endpoint,
        ParameterResource accessKey,
        ParameterResource secretKey,
        string bucketName = "camunda-backup",
        string region = "us-east-1",
        bool forcePathStyleAccess = true)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(accessKey);
        ArgumentNullException.ThrowIfNull(secretKey);

        builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_STORE", "S3");
        builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_S3_BUCKETNAME", bucketName);
        builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_S3_ENDPOINT", endpoint);
        builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_S3_REGION", region);
        builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_S3_ACCESSKEY", accessKey);
        builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_S3_SECRETKEY", secretKey);

        if (forcePathStyleAccess)
        {
            builder.WithEnvironment("ZEEBE_BROKER_DATA_BACKUP_S3_FORCEPATHSTYLEACCESS", "true");
        }

        return builder;
    }
}
