namespace Camunda.Connector.Kafka.Outbound.Model;

public record KafkaTopic(string BootstrapServers, string TopicName);
