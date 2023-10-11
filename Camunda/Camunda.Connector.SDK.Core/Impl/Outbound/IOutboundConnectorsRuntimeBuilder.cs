using Camunda.Connector.SDK.Core.Api.Outbound;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Core.Impl.Outbound;

public interface IOutboundConnectorsRuntimeBuilder
{
    IOutboundConnectorsRuntimeBuilder AddOutboundConnectorFunction<T>(Action<IServiceCollection> configure = null, string? tenatId = null) where T : class, IOutboundConnectorFunction;
}