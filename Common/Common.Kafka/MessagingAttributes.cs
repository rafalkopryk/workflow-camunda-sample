namespace Common.Kafka;

/// <summary>
///  https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
/// </summary>
public static class MessagingAttributes
{
    public const string SYSTEM = "messaging.system";

    public const string DESTINATION = "messaging.destination";

    public const string DESTINATION_KIND = "messaging.destination_kind";

    public const string KAFKA_PARTITION = "messaging.kafka.partition";

    public const string KAFKA_MESSAGE_KEY = "messaging.kafka.message_key";

    public const string MESSAGE_PAYLOAD_SIZE_BYTES = "messaging.message_payload_size_bytes";
}
