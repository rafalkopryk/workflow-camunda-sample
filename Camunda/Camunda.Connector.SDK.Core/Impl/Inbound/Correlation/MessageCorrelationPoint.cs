namespace Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;

public class MessageCorrelationPoint : ProcessCorrelationPoint
{
    public const string TYPE_NAME = "MESSAGE";

    public string MessageName { get; }

    /** FEEL expression */
    public string CorrelationKeyExpression { get; }

    public MessageCorrelationPoint(string messageName, string correlationKeyExpression)
    {
        MessageName = messageName;
        CorrelationKeyExpression = correlationKeyExpression;
    }

    public override string GetId()
    {
        return MessageName + "-" + CorrelationKeyExpression.Trim();
    }
}
