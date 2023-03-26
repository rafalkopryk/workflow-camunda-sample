using Camunda.Client;
using Camunda.Connector.Kafka;
using Camunda.Connector.SDK.Runtime.Inbound;
using Camunda.Connector.SDK.Runtime.Inbound.Importer;
using Camunda.Connector.SDK.Runtime.Outbound;
using Microsoft.Extensions.Options;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddZeebe(
           options => ctx.Configuration.GetSection("Zeebe").Bind(options),
           builder => builder
               .AddOutboundConnectorsRuntime(outboundConnectorsBuilder => outboundConnectorsBuilder
                    .AddKafkaOutboundConnectorFunction(kafkaOptions => ctx.Configuration.GetSection("EventBus").Bind(kafkaOptions)))
               .AddInboundConnectorsRuntime(inboundConnectorsBuilder => inboundConnectorsBuilder
                    .AddPathBPMNFileProcessDefinitionInspector(options => ctx.Configuration.GetSection("PathDefinitionsOptions").Bind(options))
                    .AddProcessDefinitionImporter(options => ctx.Configuration.GetSection("ProcessDefinitionsOptions").Bind(options))
                    .AddKafkaInboundConnectorFunction(kafkaOptions => ctx.Configuration.GetSection("EventBus").Bind(kafkaOptions))));
    })
    .Build();

await host.RunAsync();