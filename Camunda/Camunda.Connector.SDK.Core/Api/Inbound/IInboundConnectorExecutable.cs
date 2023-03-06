namespace Camunda.Connector.SDK.Core.Api.Inbound;

public interface IInboundConnectorExecutable
{
    Task Activate(IInboundConnectorContext context, CancellationToken cancellationToken);

    void Deactivate();
}
