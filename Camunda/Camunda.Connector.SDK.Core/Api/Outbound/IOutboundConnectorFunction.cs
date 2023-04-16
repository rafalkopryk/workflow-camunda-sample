namespace Camunda.Connector.SDK.Core.Api.Outbound;

public interface IOutboundConnectorFunction
{
    Task<object> Execute(IOutboundConnectorContext context);
}
