using System.Text.Json.Serialization;

namespace Camunda.Connector.Kafka.Inbound.Model;

public record KafkaConnectorProperties
{
    [JsonPropertyName("topic.topicName")]
    public string TopicName { get; init; }
};
