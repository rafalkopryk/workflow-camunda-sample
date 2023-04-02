using Camunda.Connector.SDK.Core.Impl.Inbound.Result;

namespace Camunda.Connector.SDK.Core.Api.Inbound;

public interface IInboundConnectorResult<out T> where T : IResponseData
{
    string Type { get; }

    string CorrelationPointId { get; }

    T ResponseData { get; }

    bool Activated { get; }

    CorrelationErrorData? ErrorData { get; }
}
