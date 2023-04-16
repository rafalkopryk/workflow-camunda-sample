namespace Camunda.Connector.SDK.Runtime.Util.Outbound;

public record ConnectorResult
{
    public Exception? Exception { get; set; }
    public object? ResponseValue { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
    public bool IsSuccess => Exception == null;
    public object GetResponseValue => Variables is null || Variables.Count == 0 ? ResponseValue : Variables;
}
