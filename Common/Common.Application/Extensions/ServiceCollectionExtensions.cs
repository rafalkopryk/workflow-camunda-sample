namespace Common.Application.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Logging;
using Camunda.Client;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);

        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = false;
        });

        var openTelemetryBuilder = services.AddOpenTelemetry();

        if (configuration.UseAzureMonitor())
        {
            openTelemetryBuilder.UseAzureMonitor(configureAzureMonitor => configureAzureMonitor.ConnectionString = configuration.GetAzureMonitorEndpoint());
        }

        if (configuration.UseOtlpExporter())
        {
            openTelemetryBuilder.UseOtlpExporter();
        }

        openTelemetryBuilder
            .WithTracing(builder => builder
                .AddAspNetCoreInstrumentation(x =>
                {
                    x.Filter = (filter) => !filter.Request.Path.Value.Contains("swagger", StringComparison.OrdinalIgnoreCase);
                    x.RecordException = true;
                })
                .AddHttpClientInstrumentation(x =>
                {
                    x.RecordException = true;
                })
                .AddGrpcClientInstrumentation()
                .AddSqlClientInstrumentation(x =>
                {
                    x.SetDbStatementForText = true;
                    x.RecordException = true;
                })
                .AddSource("Wolverine")
                .SetErrorStatusOnException()
                .AddZeebeWorkerInstrumentation()
                .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources"))
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddZeebeWorkerInstrumentation());
    }
}

