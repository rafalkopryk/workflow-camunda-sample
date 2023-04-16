namespace Camunda.Connector.Kafka.Outbound.Model;

public record KafkaConnectorRequest(KafkaTopic Topic, KafkaMessage Message);
