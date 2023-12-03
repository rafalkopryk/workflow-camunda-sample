﻿using GatewayProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Camunda.Client;

public class ZeebeBuilder : IZeebeBuilder
{
    private readonly IServiceCollection _services;

    public ZeebeBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ZeebeBuilder AddWorker<T>() where T : class, IJobHandler
    {
        var attribute = typeof(T).GetAttribute<ZeebeWorkerAttribute>();
        var serviceTaskConfiguration = new ServiceTaskConfiguration
        {
            Type = attribute.Type,
            AutoComplate = attribute.AutoComplate,
            FetchVariabeles = attribute.FetchVariabeles,
            RetryBackOffInMs = attribute.RetryBackOffInMs,
            TimeoutInMs = attribute.TimeoutInMs,
            TenatIds = attribute.TenatIds,
        };
        AddWorker<T>(serviceTaskConfiguration);
        return this;
    }

    public IZeebeBuilder AddWorker<T>(ServiceTaskConfiguration serviceTaskConfiguration, Action<IServiceCollection> configure = null) where T : class, IJobHandler
    {
        configure?.Invoke(_services);
        _services.AddSingleton(typeof(T));
        _services.AddHostedService(x =>
        {
            var client = x.GetRequiredService<Gateway.GatewayClient>();
            var serviceScope = x.GetRequiredService<IServiceScopeFactory>();
            var logger = x.GetRequiredService<ILogger<ZeebeWorker<T>>>();
            return new ZeebeWorker<T>(client, serviceScope, serviceTaskConfiguration, logger);
        });

        return this;
    }

    public IZeebeBuilder Configure(Action<IServiceCollection> configure = null)
    {
        configure?.Invoke(_services);
        return this;
    }
}