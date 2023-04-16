namespace Camunda.Connector.SDK.Core.Impl.Inbound.Result;

public class MessageCorrelationResult : AbstractInboundConnectorResult<CorrelatedMessage>
{
    public const string TYPE_NAME = "MESSAGE";

    public MessageCorrelationResult(string messageName, long messageKey)
        : base(TYPE_NAME, messageName, true, new CorrelatedMessage(messageKey), null)
    {
    }

    public MessageCorrelationResult(string messageName, CorrelationErrorData errorData)
        : base(TYPE_NAME, messageName, false, null, errorData)
    {
    }
}
