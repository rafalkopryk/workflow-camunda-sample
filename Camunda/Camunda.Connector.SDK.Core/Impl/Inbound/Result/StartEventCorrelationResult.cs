namespace Camunda.Connector.SDK.Core.Impl.Inbound.Result;

public class StartEventCorrelationResult : AbstractInboundConnectorResult<ProcessInstance>
{
    public const string TYPE_NAME = "START_EVENT";

    public StartEventCorrelationResult(long processDefinitionKey, ProcessInstance responseData)
        : base(TYPE_NAME, processDefinitionKey.ToString(), true, responseData, null)
    {
    }

    public StartEventCorrelationResult(long processDefinitionKey, CorrelationErrorData errorData)
        : base(TYPE_NAME, processDefinitionKey.ToString(), false, null, errorData)
    {
    }
}
