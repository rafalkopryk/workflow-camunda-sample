using Camunda.Connector.Kafka.Inbound.Model;

namespace Camunda.Connector.Kafka.Inbound;

internal interface IKafkaSubscription
{
    Task Subscribe(KafkaConnectorProperties properties, Func<KafkaInboundMessage, Task> callback, CancellationToken cancellationToken);
}
