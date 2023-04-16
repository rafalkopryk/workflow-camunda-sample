using Camunda.Connector.SDK.Core.Api.Inbound;

namespace Camunda.Connector.SDK.Core.Impl.Inbound.Result;

public abstract class AbstractInboundConnectorResult<T> : IInboundConnectorResult<T> where T : IResponseData
{
    public string Type { get; }

    public string CorrelationPointId { get;  }

    public bool Activated { get; }

    public T ResponseData { get; }

    public CorrelationErrorData? ErrorData { get; }

    protected AbstractInboundConnectorResult(string type, string correlationPointId, bool activated, T responseData, CorrelationErrorData? errorData)
    {
        Type = type;
        CorrelationPointId = correlationPointId;
        Activated = activated;
        ResponseData = responseData;
        ErrorData = errorData;
    }
}
