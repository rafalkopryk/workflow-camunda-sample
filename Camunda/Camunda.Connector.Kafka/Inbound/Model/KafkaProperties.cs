using System.Text.Json.Serialization;

namespace Camunda.Connector.Kafka.Inbound.Model;

public record KafkaProperties
{
    [JsonPropertyName("topicName")]
    public string TopicName { get; init; }
};
