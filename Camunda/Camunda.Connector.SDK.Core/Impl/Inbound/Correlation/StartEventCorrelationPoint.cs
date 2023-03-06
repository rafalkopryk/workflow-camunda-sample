using Camunda.Connector.SDK.Core.Api.Inbound;

namespace Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;

public class StartEventCorrelationPoint : ProcessCorrelationPoint
{
    public const string TYPE_NAME = "START_EVENT";

    public string BpmnProcessId { get; }
    public int Version { get; }
    public long ProcessDefinitionKey { get; }

    public StartEventCorrelationPoint(string bpmnProcessId, int version, long processDefinitionKey)
    {
        BpmnProcessId = bpmnProcessId;
        Version = version;
        ProcessDefinitionKey = processDefinitionKey;
    }

    public override string GetId()
    {
        return BpmnProcessId + "-" + Version + "-" + ProcessDefinitionKey;
    }
}
