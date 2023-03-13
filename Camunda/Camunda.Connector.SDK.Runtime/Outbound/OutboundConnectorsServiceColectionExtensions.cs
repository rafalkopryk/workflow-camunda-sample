using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Camunda.Connector.SDK.Core.Impl.Outbound;
using Camunda.Connector.SDK.Runtime.Util.Outbound;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Runtime.Outbound;

public static class OutboundConnectorsServiceColectionExtensions
{
    public static IZeebeBuilder AddOutboundConnectorsRuntime(this IZeebeBuilder zeebeBuilder, Action<OutboundConnectorsRuntimeBuilder> configure)
    {
        zeebeBuilder.Configure(serviceCollection =>
        {
            var builder = new OutboundConnectorsRuntimeBuilder(zeebeBuilder, serviceCollection);
            configure?.Invoke(builder);
        });

        return zeebeBuilder;
    }
}

public class OutboundConnectorsRuntimeBuilder : IOutboundConnectorsRuntimeBuilder
{
    private readonly IZeebeBuilder _zeebeBuilder;

    private readonly IServiceCollection _serviceCollecion;

    public OutboundConnectorsRuntimeBuilder(IZeebeBuilder zeebeBuilder, IServiceCollection serviceCollecion)
    {
        _zeebeBuilder = zeebeBuilder;
        _serviceCollecion = serviceCollecion;
    }

    public IOutboundConnectorsRuntimeBuilder AddOutboundConnectorFunction<T>(Action<IServiceCollection> configure = null) where T : class, IOutboundConnectorFunction
    {
        var attribute = typeof(T).GetAttribute<OutboundConnectorAttribute>();
        var serviceTaskConfiguration = new ServiceTaskConfiguration
        {
            Type = attribute.Type,
            AutoComplate = false,
            FetchVariabeles = attribute.InputVariables,
        };

        _serviceCollecion.AddScoped(typeof(T));
        _zeebeBuilder.AddWorker<ConnectorJobHandler<T>>(serviceTaskConfiguration, configure);

        return this;
    }
}