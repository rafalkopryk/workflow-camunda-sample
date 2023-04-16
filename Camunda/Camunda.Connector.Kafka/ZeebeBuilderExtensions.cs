using Camunda.Connector.Kafka.Inbound;
using Camunda.Connector.Kafka.Outbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Outbound;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.Kafka;
public static class ZeebeBuilderExtensions
{
    public static IOutboundConnectorsRuntimeBuilder AddKafkaOutboundConnectorFunction(this IOutboundConnectorsRuntimeBuilder builder, Action<ProducerConfig> producerOptions)
    {
        builder.AddOutboundConnectorFunction<KafkaConnectorFunction>(serviceCollections =>
        {
            var configuration = new ProducerConfig();
            serviceCollections.Configure(producerOptions);
        });

        return builder;
    }

    public static IInboundConnectorsRuntimeBuilder AddKafkaInboundConnectorFunction(this IInboundConnectorsRuntimeBuilder builder, Action<ConsumerConfig> consumerOptions)
    {
        builder.AddInboundConnectorExecutable<KafkaExecutable>(serviceCollections =>
        {
            serviceCollections.Configure(consumerOptions);
            serviceCollections.AddScoped<IKafkaSubscription, KafkaSubscription>();
        });

        return builder;
    }
}
