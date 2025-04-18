﻿using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class CamundaBuilderExtensions
{
    private const int DefaultGrpcPort = 26500;
    private const int DefaultRestPort = 8080;
    
    public static IResourceBuilder<CamundaResource> WithOperate(this IResourceBuilder<CamundaResource> builder, ReferenceExpression elasticConnectionString , int port = 8085)
    {
        var zeebeConnectionString = ReferenceExpression.Create($"{builder.Resource.GrpcEndpoint.Property(EndpointProperty.Host)}:{builder.Resource.GrpcEndpoint.Property(EndpointProperty.Port)}");

        builder
            .WithEnvironment("CAMUNDA_OPERATE_ZEEBE_GATEWAYADDRESS", zeebeConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_ELASTICSEARCH_URL", elasticConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_ZEEBEELASTICSEARCH_URL", elasticConnectionString)
            .WithEnvironment("CAMUNDA_OPERATE_IMPORTER_ENABLED", "false")
            .WithEnvironment("SPRING_PROFILES_ACTIVE", "operate,broker,consolidated-auth");

        return builder;
    }

    public static IResourceBuilder<CamundaResource> AddCamunda(this IDistributedApplicationBuilder builder, string name, ReferenceExpression? elasticConnectionString = null ,int? restPort = 8089)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var zeebeContainer = new CamundaResource(name);
        var resource = builder
            .AddResource(zeebeContainer)
            .WithHttpEndpoint(port: DefaultGrpcPort, targetPort: DefaultGrpcPort, CamundaResource.GprcEndpointName)
            .WithHttpEndpoint(port: restPort, targetPort: DefaultRestPort, name: CamundaResource.RestEndpointName)
            .WithHttpEndpoint(port: 9600, targetPort: 9600, name: "internal")
            .WithImage("camunda/camunda", "8.8.0-alpha2")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGECOMMANDWATERMARK", "0.998")
            .WithEnvironment("ZEEBE_BROKER_DATA_DISKUSAGEREPLICATIONWATERMARK", "0.999")

            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_MODE", "none")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHENTICATION_UNPROTECTEDAPI", "true")
            .WithEnvironment("CAMUNDA_SECURITY_AUTHORIZATIONS_ENABLED", "false")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_EMAIL", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_NAME", "Demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_PASSWORD", "demo")
            .WithEnvironment("CAMUNDA_SECURITY_INITIALIZATION_USERS[0]_EMAIL", "demo@demo.com")

            .WithEnvironment("SPRING_PROFILES_ACTIVE", "broker,consolidated-auth");


        if (elasticConnectionString != null)
        {
            resource.WithDatabase(elasticConnectionString);
        }

        resource.WithHttpHealthCheck("actuator/health/readiness", 200, "internal");
        return resource;
    }
    
    public static IResourceBuilder<CamundaResource> WithElasticExporter(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString)
    {
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_CLASSNAME", "io.camunda.zeebe.exporter.ElasticsearchExporter");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_ARGS_URL", elasticConnectionString);
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_ARGS_BULK_SIZE", "1000");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_ELASTICSEARCH_ARGS_BULK_DELAY", "1");

        return builder;
    }

    public static IResourceBuilder<CamundaResource> WithCamundaExporter(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString)
    {
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_CLASSNAME", "io.camunda.exporter.CamundaExporter");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_CONNECT_URL", elasticConnectionString);
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_CONNECT_TYPE", "elasticsearch");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_BULK_SIZE", "1000");
        builder.WithEnvironment("ZEEBE_BROKER_EXPORTERS_CAMUNDAEXPORTER_ARGS_BULK_DELAY", "1");

        return builder;
    }

    public static IResourceBuilder<CamundaResource> WithDataVolume(this IResourceBuilder<CamundaResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name, "/usr/local/zeebe/data", isReadOnly);
    }

    public static IResourceBuilder<T> WithZeebeReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<CamundaResource> source) where T : IResourceWithEnvironment
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

    public static IResourceBuilder<CamundaResource> WithDatabase(this IResourceBuilder<CamundaResource> builder, ReferenceExpression? elasticConnectionString)
    {
        builder.WithEnvironment("CAMUNDA_DATABASE_TYPE", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATABASE_CLUSTERNAME", "elasticsearch");
        builder.WithEnvironment("CAMUNDA_DATABASE_URL", elasticConnectionString);
        //builder.WithEnvironment("CAMUNDA_REST_QUERY_ENABLED", "true");

        builder.WithCamundaExporter(elasticConnectionString);

        return builder;
    }
}

public sealed class CamundaResource(string name) : ContainerResource(name), IResourceWithConnectionString
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