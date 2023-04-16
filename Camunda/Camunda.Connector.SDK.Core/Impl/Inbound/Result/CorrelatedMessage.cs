using Camunda.Connector.SDK.Core.Api.Inbound;

namespace Camunda.Connector.SDK.Core.Impl.Inbound.Result;

public record CorrelatedMessage(long MessageKey) : IResponseData;
