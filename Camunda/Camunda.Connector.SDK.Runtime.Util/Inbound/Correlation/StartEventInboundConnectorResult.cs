using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using GatewayProtocol;

namespace Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;

public class StartEventInboundConnectorResult : InboundConnectorResult
{
    public override CreateProcessInstanceResponse ResponseData { get; }

    public StartEventInboundConnectorResult(CreateProcessInstanceResponse result)
    : base(StartEventCorrelationPoint.TYPE_NAME, result.ProcessInstanceKey.ToString(), result)
    {
        ResponseData = result;
    }
}


