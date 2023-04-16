using Camunda.Client;
using Camunda.Client.Operate;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Runtime.Inbound.Importer;
using Camunda.Connector.SDK.Runtime.Inbound.Importer.File;
using Camunda.Connector.SDK.Runtime.Inbound.Lifecycle;
using Camunda.Connector.SDK.Runtime.Util.Inbound;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.WebRequestMethods;

namespace Camunda.Connector.SDK.Runtime.Inbound;

public static class InboundConnectorsServiceColectionExtensions
{
    public static IZeebeBuilder AddInboundConnectorsRuntime(this IZeebeBuilder zeebeBuilder, Action<InboundConnectorsRuntimeBuilder> configure)
    {
        zeebeBuilder.Configure(services =>
        {
            services.AddSingleton<InboundCorrelationHandler>();
            services.AddSingleton<IInboundConnectorFactoryConnectorFactory, DefaultInboundConnectorFactory>();
            services.AddSingleton<InboundConnectorManager>();

            var builder = new InboundConnectorsRuntimeBuilder(services);
            configure?.Invoke(builder);
        });

        return zeebeBuilder;
    }
}

public class InboundConnectorsRuntimeBuilder : IInboundConnectorsRuntimeBuilder
{
    private readonly IServiceCollection _services;

    public InboundConnectorsRuntimeBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public InboundConnectorsRuntimeBuilder AddPathBPMNFileProcessDefinitionInspector(Action<PathFileProviderOptions> configure) 
    {
        _services.Configure(configure);

        _services.AddSingleton<IBpmnProvider, PathFileProvider>();
        _services.AddSingleton<IProcessDefinitionInspector, BPMNFileProcessDefinitionInspector>();
        return this;
    }

    public InboundConnectorsRuntimeBuilder AddOperateBPMNFileProcessDefinitionInspector(Action<PathFileProviderOptions> configure)
    {
        _services.AddHttpClient<IOperateClient, OperateClient>(client =>
        {
            client.BaseAddress = new Uri("http://operate:8080/");
        });
        _services.AddSingleton<IBpmnProvider, OperateFileProvider>();
        _services.AddSingleton<IProcessDefinitionInspector, BPMNFileProcessDefinitionInspector>();
        return this;
    }

    public InboundConnectorsRuntimeBuilder AddProcessDefinitionImporter(Action<ProcessDefinitionOptions> configure)
    {
        _services.Configure(configure);
        _services.AddHostedService<ProcessDefinitionImporter>();

        return this;
    }

    public IInboundConnectorsRuntimeBuilder AddInboundConnectorExecutable<T>(Action<IServiceCollection> configure = null) where T : class, IInboundConnectorExecutable
    {
        configure?.Invoke(_services);

        _services.AddScoped(typeof(T));

        var inboundConnectorAttribute = typeof(T).GetAttribute<InboundConnectorAttribute>();
        var inboundConnectorConfigurable = new InboundConnectorConfiguration(inboundConnectorAttribute.Name, inboundConnectorAttribute.Type, typeof(T));
        _services.AddSingleton(inboundConnectorConfigurable);

        return this;
    }
}