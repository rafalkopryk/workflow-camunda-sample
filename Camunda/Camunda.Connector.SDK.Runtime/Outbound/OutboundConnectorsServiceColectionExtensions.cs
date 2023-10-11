using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Camunda.Connector.SDK.Core.Impl.Outbound;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using Camunda.Connector.SDK.Runtime.Util.Outbound;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Camunda.Connector.SDK.Runtime.Outbound;

public static class OutboundConnectorsServiceColectionExtensions
{
    public static IZeebeBuilder AddOutboundConnectorsRuntime(this IZeebeBuilder zeebeBuilder, Action<IOutboundConnectorsRuntimeBuilder> configure)
    {
        zeebeBuilder.Configure(serviceCollection =>
        {
            serviceCollection.TryAddSingleton<IJsonTransformerEngine, ConsJsonTransformerEngine>();

            var builder = new OutboundConnectorsRuntimeBuilder(zeebeBuilder, serviceCollection);
            configure?.Invoke(builder);
        });

        return zeebeBuilder;
    }
}

internal class OutboundConnectorsRuntimeBuilder : IOutboundConnectorsRuntimeBuilder
{
    private readonly IZeebeBuilder _zeebeBuilder;

    private readonly IServiceCollection _serviceCollecion;

    public OutboundConnectorsRuntimeBuilder(IZeebeBuilder zeebeBuilder, IServiceCollection serviceCollecion)
    {
        _zeebeBuilder = zeebeBuilder;
        _serviceCollecion = serviceCollecion;
    }

    public IOutboundConnectorsRuntimeBuilder AddOutboundConnectorFunction<T>(Action<IServiceCollection> configure = null, string tenatId = null) where T : class, IOutboundConnectorFunction
    {
        var attribute = typeof(T).GetAttribute<OutboundConnectorAttribute>();
        var serviceTaskConfiguration = new ServiceTaskConfiguration
        {
            Type = attribute.Type,
            AutoComplate = false,
            FetchVariabeles = attribute.InputVariables,
            TenatIds = tenatId != null ? new[]{ tenatId } : Array.Empty<string>(),
        };

        _serviceCollecion.AddSingleton(typeof(T));
        _zeebeBuilder.AddWorker<ConnectorJobHandler<T>>(serviceTaskConfiguration, configure);

        return this;
    }
}