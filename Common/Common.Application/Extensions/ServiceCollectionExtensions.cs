namespace Common.Application.Extensions;

using Common.Application.Zeebe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
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
            .SetErrorStatusOnException()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
                    .AddTelemetrySdk())
            .AddConsoleExporter()
            .AddOtlpExporter(configure =>
            {
                configure.Endpoint = new Uri(configuration.GetSection("otel:url").Value);
            }));

        services.Configure<ZeebeOptions>(options => configuration.GetSection("ZEEBE").Bind(options));
        services.AddSingleton<IZeebeService, ZeebeService>();

        var zeebejobProvider = new ZeebeJobHandlerProvider();
        zeebejobProvider.RegisterZeebeJobs();

        services.AddSingleton<IZeebeJobHandlerProvider>(zeebejobProvider);

        services.AddHostedService<ZeebeWorker>();
    }
}

