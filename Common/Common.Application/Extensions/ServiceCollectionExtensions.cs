﻿namespace Common.Application.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Camunda.Client;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, ResourceBuilder resourceBuilder)
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
                    x.SetDbStatementForStoredProcedure = true;
                    x.RecordException = true;
                })
                .AddSource("Wolverine")
                .SetErrorStatusOnException()
                .SetResourceBuilder(resourceBuilder)
                .AddZeebeWorkerInstrumentation()
                .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources"))
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .SetResourceBuilder(resourceBuilder)
                .AddZeebeWorkerInstrumentation());
    }
}

