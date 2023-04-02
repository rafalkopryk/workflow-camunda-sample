namespace Camunda.Connector.SDK.Core.Api.Error;

public record BpmnError
{
    public string Code { get; init; }
    public string Message { get; init; }

    public bool HasCode()
    {
        return Code != null;
    }
}
