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

    public const string CAMUNDA_PROCESS_INSTANCE_KEY = "messaging.camunda.process_instance_key";
    public const string CAMUNDA_BPMN_PROCESS_ID = "messaging.camunda.bpmn_process_id";
    public const string CAMUNDA_ELEMENT_ID = "messaging.camunda.element_id";
}
