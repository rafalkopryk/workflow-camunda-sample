namespace Camunda.Connector.Kafka.Model;

public record KafkaTopic(string BootstrapServers, string TopicName);
