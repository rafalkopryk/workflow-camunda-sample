﻿using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Runtime.Inbound.Importer;
using Camunda.Connector.SDK.Runtime.Inbound.Importer.File;
using Camunda.Connector.SDK.Runtime.Inbound.Importer.Mock;
using Camunda.Connector.SDK.Runtime.Inbound.Lifecycle;
using Camunda.Connector.SDK.Runtime.Util.Inbound;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Runtime.Inbound;

public static class InboundConnectorsServiceColectionExtensions
{
    public static IZeebeBuilder AddInboundConnectorsRuntime(this IZeebeBuilder zeebeBuilder, Action<InboundConnectorsRuntimeBuilder> configure)
    {
        zeebeBuilder.Configure(services =>
        {
            services.AddScoped<InboundCorrelationHandler>();
            services.AddScoped<IInboundConnectorFactoryConnectorFactory, DefaultInboundConnectorFactory>();

            services.AddScoped<InboundConnectorManager>();

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

    public InboundConnectorsRuntimeBuilder AddMockProcessDefinitionInspector()
    {
        _services.AddScoped<IProcessDefinitionInspector, MockProcessDefinitionInspector>();
        return this;
    }

    public InboundConnectorsRuntimeBuilder AddPathBPMNFileProcessDefinitionInspector(Action<PathFileProviderOptions> configure) 
    {
        _services.Configure(configure);

        _services.AddSingleton<IBpmnProvider, PathFileProvider>();
        _services.AddScoped<IProcessDefinitionInspector, BPMNFileProcessDefinitionInspector>();
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

        _services.AddScoped(typeof(T));

        var inboundConnectorAttribute = typeof(T).GetAttribute<InboundConnectorAttribute>();
        var inboundConnectorConfigurable = new InboundConnectorConfiguration(inboundConnectorAttribute.Name, inboundConnectorAttribute.Type, typeof(T));
        _services.AddSingleton(inboundConnectorConfigurable);

        return this;
    }
}