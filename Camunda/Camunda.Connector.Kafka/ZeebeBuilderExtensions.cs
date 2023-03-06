using Camunda.Client;
using Camunda.Connector.Kafka.Inbound;
using Camunda.Connector.Kafka.Model;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Runtime.Util.Outbound;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.Kafka;
public static class ZeebeBuilderExtensions
{
    public static ZeebeBuilder AddKafkaOutboundConnectorFunction(this ZeebeBuilder builder, IServiceCollection serviceCollections, Action<ProducerConfig> producerOptions)
    {
        var producerConfig = new ProducerConfig();
        producerOptions?.Invoke(producerConfig);

        var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        serviceCollections.AddSingleton(producer);

        serviceCollections.AddScoped<KafkaConnectorFunction>();
        var attribute = typeof(KafkaConnectorFunction).GetAttribute<OutboundConnectorAttribute>();
        builder.AddWorker<ConnectorJobHandler<KafkaConnectorFunction>>(new ZeebeWorkerAttribute
        {
            AutoComplate = false,
            Type = attribute.Type,
            FetchVariabeles = attribute.InputVariables,
        });

        return builder;
    }

    public static IInboundConnectorsRuntimeBuilder AddKafkaInboundConnectorFunction(this IInboundConnectorsRuntimeBuilder builder, Action<ConsumerConfig> consumerOptions)
    {
        builder.AddInboundConnectorExecutable<KafkaExecutable>(serviceCollections =>
        {
            serviceCollections.Configure<ConsumerConfig>(consumerOptions);
        });

        return builder;
    }
}
