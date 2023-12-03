using Camunda.Client;
using Camunda.Connector.Kafka;
using Camunda.Connector.SDK.Runtime.Inbound;
using Camunda.Connector.SDK.Runtime.Outbound;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Utility.KafkaConnector.UseCasaes;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = false;
});

var configuration = builder.Configuration;

builder.Services.AddZeebe(
    options => builder.Configuration.GetSection("Zeebe").Bind(options),
    builder => builder
        .AddWorker<ThrowErrorTaskHandler>()
        .AddOutboundConnectorsRuntime(outboundConnectorsBuilder => outboundConnectorsBuilder
            .AddKafkaOutboundConnectorFunction(kafkaOptions => configuration.GetSection("EventBus").Bind(kafkaOptions)))
        .AddInboundConnectorsRuntime(inboundConnectorsBuilder => inboundConnectorsBuilder
            //.AddOperateBPMNFileProcessDefinitionInspector(options => { })
            .AddPathBPMNFileProcessDefinitionInspector(options => configuration.GetSection("PathDefinitionsOptions").Bind(options))
            .AddProcessDefinitionImporter(options => configuration.GetSection("ProcessDefinitionsOptions").Bind(options))
            .AddKafkaInboundConnectorFunction(kafkaOptions => configuration.GetSection("EventBus").Bind(kafkaOptions))));

if (builder.Configuration.GetValue<bool>("otel:enabled"))
{

    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService("Utility.KafkaConnector", serviceVersion: "1.0.0")
        .AddTelemetrySdk();

    builder.Services.AddOpenTelemetry()
        .WithTracing(builder => builder
            .AddGrpcClientInstrumentation()
            .AddKafkaConnectorInstrumentation()
            .AddZeebeWorkerInstrumentation()
            .SetErrorStatusOnException()
            .SetResourceBuilder(resourceBuilder)
            .AddOtlpExporter(configure => configure.Endpoint = new Uri(configuration.GetSection("OTEL:EXPORTER:OTLP:ENDPOINT").Value)))
        .WithMetrics(builder => builder
            .SetResourceBuilder(resourceBuilder)
            .AddZeebeWorkerInstrumentation()
            .AddOtlpExporter(configure => configure.Endpoint = new Uri(configuration.GetSection("OTEL:EXPORTER:OTLP:ENDPOINT").Value)));

    builder.Logging.AddOpenTelemetry(options => options
        .SetResourceBuilder(resourceBuilder)
        .AddOtlpExporter(configure =>
        {
            configure.Endpoint = new Uri(configuration.GetSection("OTEL:EXPORTER:OTLP:ENDPOINT").Value);
        }));
}

builder.Services.AddHostedService<DeployBPMNDefinitionService>();

var host = builder.Build();
await host.RunAsync();