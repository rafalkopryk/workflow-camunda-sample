using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Camunda.Connector.SDK.Runtime.Util.Outbound;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Runtime.Util;
public static class ZeebeBuilderExtensions
{
    public static ZeebeBuilder AddOutboundConnectorFunction<T>(this ZeebeBuilder builder, IServiceCollection serviceCollections) where T : IOutboundConnectorFunction
    {
        serviceCollections.AddScoped(typeof(T));
        var attribute = typeof(T).GetAttribute<OutboundConnectorAttribute>();
        builder.AddWorker<ConnectorJobHandler<T>>(new ZeebeWorkerAttribute
        {
            AutoComplate = false,
            Type = attribute.Type,
            FetchVariabeles = attribute.InputVariables,
        });

        return builder;
    }
}
