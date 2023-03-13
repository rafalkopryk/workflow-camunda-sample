using System.Text.Json.Serialization;

namespace Camunda.Connector.Kafka.Inbound.Model;

public record KafkaProperties
{
    [JsonPropertyName("subscription.topic")]
    public string SubscriptionTopic { get; init; }
};
