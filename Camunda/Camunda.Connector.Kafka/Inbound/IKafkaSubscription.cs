using Camunda.Connector.Kafka.Inbound.Model;

namespace Camunda.Connector.Kafka.Inbound;

internal interface IKafkaSubscription
{
    Task ProduceEvent(KafkaProperties properties, Func<KafkaSubscriptionEvent, Task> callback, CancellationToken cancellationToken);
}
