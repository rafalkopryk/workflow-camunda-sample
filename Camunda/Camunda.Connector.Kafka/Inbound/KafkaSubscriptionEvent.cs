namespace Camunda.Connector.Kafka.Inbound;

internal record KafkaSubscriptionEvent
(
   string Sender,
   int Code,
   object Message
);
