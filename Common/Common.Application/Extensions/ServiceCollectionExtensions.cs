namespace Common.Application.Extensions;

using Common.Application.MediatR;
using Common.Application.Zeebe;
using Common.Zeebe;
using global::MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(BusinessRuleValidationExceptionProcessorBehavior<,>));

        services.AddOpenTelemetryTracing(builder => builder
            .AddAspNetCoreInstrumentation(x =>
            {
                x.Filter = (filter) =>
                {
                    var swagger = filter.Request.Path.Value.Contains("swagger", StringComparison.OrdinalIgnoreCase);
                    var result = swagger;
                    return !result;
                };
                x.RecordException = true;
            })
            .AddGrpcClientInstrumentation()
            .AddSqlClientInstrumentation(x=>
            {
                x.SetDbStatementForText = true;
                x.SetDbStatementForStoredProcedure = true;
                x.RecordException = true;
            })
            .SetErrorStatusOnException()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
                    .AddTelemetrySdk())
            .AddOtlpExporter(configure =>
            {
                configure.Endpoint = new Uri(configuration.GetSection("otel:url").Value);
            }));
    }
}

