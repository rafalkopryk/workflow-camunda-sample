namespace Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;

public class MessageCorrelationPoint : ProcessCorrelationPoint
{
    public const string TYPE_NAME = "MESSAGE";

    public string MessageName { get; }

    public MessageCorrelationPoint(string messageName)
    {
        MessageName = messageName;
    }

    public override string GetId()
    {
        return MessageName;
    }
}
