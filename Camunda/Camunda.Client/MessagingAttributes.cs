namespace Camunda.Client;

/// <summary>
///  https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/
/// </summary>
public static class MessagingAttributes
{
    public const string SYSTEM = "messaging.system";

    public const string DESTINATION_NAME = "messaging.destination.name";
    public const string DESTINATION = "messaging.destination";

    public const string OPERATION = "messaging.operation";

    public const string ZEEBE_PROCESS_INSTANCE_KEY = "messaging.zeebe.process_instance_key";
    public const string ZEEBE_BPMN_PROCESS_ID = "messaging.zeebe.bpmn_process_id";
    public const string ZEEBE_ELEMENT_ID = "messaging.zeebe.element_id";
}
