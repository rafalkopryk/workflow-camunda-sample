using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Runtime.Inbound.Importer;
using Camunda.Connector.SDK.Runtime.Inbound.Lifecycle;
using Camunda.Connector.SDK.Runtime.Util.Inbound;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Runtime.Inbound;

public static class ServiceColectionExtensions
{
    public static void AddInboundConnectorsRuntime(this IServiceCollection services, Action<InboundConnectorsRuntimeBuilder> configure)
    {
        services.AddScoped<InboundCorrelationHandler>();
        services.AddScoped<IInboundConnectorFactoryConnectorFactory, DefaultInboundConnectorFactory>();

        services.AddScoped<InboundConnectorManager>();

        var builder = new InboundConnectorsRuntimeBuilder(services);
        configure?.Invoke(builder);
    }
}

public class InboundConnectorsRuntimeBuilder : IInboundConnectorsRuntimeBuilder
{
    private readonly IServiceCollection _services;

    public InboundConnectorsRuntimeBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public InboundConnectorsRuntimeBuilder AddProcessDefinitionInspector<T>() where T : class, IProcessDefinitionInspector
    {
        _services.AddScoped<IProcessDefinitionInspector, T>();
        return this;
    }

    public InboundConnectorsRuntimeBuilder AddProcessDefinitionImporter(Action<ProcessDefinitionOptions> configure)
    {
        _services.Configure<ProcessDefinitionOptions>(configure);
        _services.AddHostedService<ProcessDefinitionImporter>();

        return this;
    }

    public IInboundConnectorsRuntimeBuilder AddInboundConnectorExecutable<T>(Action<IServiceCollection> configure = null) where T : class, IInboundConnectorExecutable
    {
        configure?.Invoke(_services);

        _services.AddTransient(typeof(T));

        var inboundConnectorAttribute = typeof(T).GetAttribute<InboundConnectorAttribute>();
        var inboundConnectorConfigurable = new InboundConnectorConfiguration(inboundConnectorAttribute.Name, inboundConnectorAttribute.Type, typeof(T));
        _services.AddSingleton(inboundConnectorConfigurable);

        return this;
    }
}