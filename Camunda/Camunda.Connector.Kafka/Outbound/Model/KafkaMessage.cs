namespace Camunda.Connector.Kafka.Outbound.Model;

public record KafkaMessage(object Key, object Value);