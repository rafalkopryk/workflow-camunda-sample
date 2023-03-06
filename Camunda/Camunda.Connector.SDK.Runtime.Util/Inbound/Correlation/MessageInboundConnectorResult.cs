using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using GatewayProtocol;

namespace Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;

public class MessageInboundConnectorResult : InboundConnectorResult
{
    public override PublishMessageResponse ResponseData { get; }

    public MessageInboundConnectorResult(PublishMessageResponse publishMessageResponse, string correlationKey)
        : base(MessageCorrelationPoint.TYPE_NAME, correlationKey, publishMessageResponse)
    {
        ResponseData = publishMessageResponse;
    }
}


