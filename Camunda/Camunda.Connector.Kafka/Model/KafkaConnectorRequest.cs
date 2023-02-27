namespace Camunda.Connector.Kafka.Model;

public record KafkaConnectorRequest(KafkaTopic Topic, KafkaMessage Message);
