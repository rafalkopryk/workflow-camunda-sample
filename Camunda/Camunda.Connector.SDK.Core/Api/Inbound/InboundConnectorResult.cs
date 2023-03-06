namespace Camunda.Connector.SDK.Core.Api.Inbound;

public abstract class InboundConnectorResult
{
    public string Type { get; }
    public string Id { get; }
    public virtual object ResponseData { get; }

    protected InboundConnectorResult(string type, string id, object responseData)
    {
        Type = type;
        Id = id;
        ResponseData = responseData;
    }
}
