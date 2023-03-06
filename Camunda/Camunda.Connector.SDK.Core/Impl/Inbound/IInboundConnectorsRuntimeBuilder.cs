using Camunda.Connector.SDK.Core.Api.Inbound;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Core.Impl.Inbound;

public interface IInboundConnectorsRuntimeBuilder
{
    IInboundConnectorsRuntimeBuilder AddInboundConnectorExecutable<T>(Action<IServiceCollection> configure = null) where T : class, IInboundConnectorExecutable;
}