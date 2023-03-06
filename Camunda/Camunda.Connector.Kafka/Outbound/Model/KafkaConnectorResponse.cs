namespace Camunda.Connector.Kafka.Outbound.Model;

public record KafkaConnectorResponse(
    string Topic,
    long Timestamp,
    long Offset,
    int partition);
