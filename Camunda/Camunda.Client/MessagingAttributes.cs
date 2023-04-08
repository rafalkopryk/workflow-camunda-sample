namespace Camunda.Client;

/// <summary>
///  https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
/// </summary>
public static class MessagingAttributes
{
    public const string SYSTEM = "messaging.system";

    public const string DESTINATION = "messaging.destination";

    public const string DESTINATION_KIND = "messaging.destination_kind";
}
