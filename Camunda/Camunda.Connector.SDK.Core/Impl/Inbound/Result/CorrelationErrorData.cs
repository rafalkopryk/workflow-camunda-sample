namespace Camunda.Connector.SDK.Core.Impl.Inbound.Result;

public record CorrelationErrorData(CorrelationErrorReason Reason, string? Message = null);
