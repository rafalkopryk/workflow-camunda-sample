using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;

namespace Camunda.Connector.SDK.Runtime.Util.Inbound;

public interface IInboundConnectorFactoryConnectorFactory: ConnectorFactory<IInboundConnectorExecutable, InboundConnectorConfiguration>
{

}
