using Camunda.Client;
using Camunda.Connector.Kafka;
using Camunda.Connector.SDK.Runtime.Inbound;
using Camunda.Connector.SDK.Runtime.Outbound;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddZeebe(
           options => ctx.Configuration.GetSection("Zeebe").Bind(options),
           builder => builder
               .AddOutboundConnectorsRuntime(outboundConnectorsBuilder => outboundConnectorsBuilder
                    .AddKafkaOutboundConnectorFunction(kafkaOptions => ctx.Configuration.GetSection("EventBus").Bind(kafkaOptions)))
               .AddInboundConnectorsRuntime(inboundConnectorsBuilder => inboundConnectorsBuilder
                    //.AddOperateBPMNFileProcessDefinitionInspector(options => { })
                    .AddPathBPMNFileProcessDefinitionInspector(options => ctx.Configuration.GetSection("PathDefinitionsOptions").Bind(options))
                    .AddProcessDefinitionImporter(options => ctx.Configuration.GetSection("ProcessDefinitionsOptions").Bind(options))
                    .AddKafkaInboundConnectorFunction(kafkaOptions => ctx.Configuration.GetSection("EventBus").Bind(kafkaOptions))));

        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddGrpcClientInstrumentation()
                .AddKafkaConnectorInstrumentation()
                .AddZeebeWorkerInstrumentation()
                .SetErrorStatusOnException()
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: "Utility.KafkaConnector", serviceVersion: "1.0.0")
                        .AddTelemetrySdk())
                .AddOtlpExporter(configure =>
                {
                    configure.Endpoint = new Uri(ctx.Configuration.GetSection("otel:url").Value);
                }));


        services.AddHostedService<DeployBPMNDefinitionService>();
    })
    .Build();

await host.RunAsync();