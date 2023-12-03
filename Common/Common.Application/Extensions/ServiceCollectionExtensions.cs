namespace Common.Application.Extensions;

using Common.Application.MediatR;
using global::MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Common.Kafka;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

public static class ServiceCollectionExtensions
{
    public static void ConfigureLogger(this ILoggingBuilder loggingBuilder, IConfiguration configuration, ResourceBuilder resourceBuilder)
    {
        if (configuration.GetValue<bool>("otel:enabled"))
        {
            loggingBuilder.AddOpenTelemetry(options => options
            .SetResourceBuilder(resourceBuilder)
            .AddOtlpExporter(configure =>
            {
                configure.Endpoint = new Uri(configuration.GetSection("OTEL:EXPORTER:OTLP:ENDPOINT").Value);
            }));
        }
    }

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, ResourceBuilder resourceBuilder)
    {
        services.AddSingleton(DateTimeProvider.Shared);

        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = false;
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BusinessRuleValidationExceptionProcessorBehavior<,>));

        if (configuration.GetValue<bool>("otel:enabled"))
        {
            services.AddOpenTelemetry()
                .WithTracing(builder => builder
                    .AddAspNetCoreInstrumentation(x =>
                    {
                        x.Filter = (filter) => !filter.Request.Path.Value.Contains("swagger", StringComparison.OrdinalIgnoreCase);
                        x.RecordException = true;
                    })
                    .AddKafkaInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddSqlClientInstrumentation(x =>
                    {
                        x.SetDbStatementForText = true;
                        x.SetDbStatementForStoredProcedure = true;
                        x.RecordException = true;
                    })
                    .AddElasticsearchClientInstrumentation(options => options.SetDbStatementForRequest = true)
                    .SetErrorStatusOnException()
                    .SetResourceBuilder(resourceBuilder)
                    .AddOtlpExporter(configure =>
                    {
                        configure.Endpoint = new Uri(configuration.GetSection("OTEL:EXPORTER:OTLP:ENDPOINT").Value);
                    }))
                .WithMetrics(builder => builder
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .SetResourceBuilder(resourceBuilder)
                    .AddOtlpExporter(configure =>
                    {
                        configure.Endpoint = new Uri(configuration.GetSection("OTEL:EXPORTER:OTLP:ENDPOINT").Value);
                    }));

        }
    }
}

