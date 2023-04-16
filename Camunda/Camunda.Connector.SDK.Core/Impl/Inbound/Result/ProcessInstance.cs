using Camunda.Connector.SDK.Core.Api.Inbound;

namespace Camunda.Connector.SDK.Core.Impl.Inbound.Result;

public record ProcessInstance(
    long ProcessInstanceKey,
    string BpmnProcessId,
    long ProcessDefinitionKey,
    int Version
) : IResponseData;