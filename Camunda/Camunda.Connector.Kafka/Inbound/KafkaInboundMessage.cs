namespace Camunda.Connector.Kafka.Inbound;

internal record KafkaInboundMessage
(
   string Key,
   string RawValue,
   object Value
);