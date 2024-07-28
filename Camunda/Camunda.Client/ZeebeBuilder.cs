using GatewayProtocol;
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
        var attribute = typeof(T).GetAttribute<JobWorkerAttribute>();
        var serviceTaskConfiguration = new ServiceTaskConfiguration
        {
            Type = attribute.Type,
            AutoComplate = attribute.AutoComplete,
            FetchVariabeles = attribute.FetchVariabeles,
            RetryBackOffInMs = attribute.RetryBackOffInMs,
            TimeoutInMs = attribute.TimeoutInMs,
            TenatIds = attribute.TenatIds,
            PoolingDelayInMs = attribute.PoolingDelayInMs,
            PoolingMaxJobsToActivate = attribute.PoolingMaxJobsToActivate,
            PoolingRequestTimeoutInMs = attribute.PoolingRequestTimeoutInMs,
            UseStream = attribute.UseStream,
            StreamTimeoutInSec = attribute.StreamTimeoutInSec,
        };
        AddWorker<T>(serviceTaskConfiguration);
        return this;
    }

    public IZeebeBuilder AddWorker<T>(ServiceTaskConfiguration serviceTaskConfiguration, Action<IServiceCollection> configure = null) where T : class, IJobHandler
    {
        configure?.Invoke(_services);
        _services.AddScoped(typeof(T));

        if (serviceTaskConfiguration.UseStream)
        {
            _services.AddHostedService(x =>
            {
                var client = x.GetRequiredService<Gateway.GatewayClient>();
                var logger = x.GetRequiredService<ILogger<StreamZeebeWorker<T>>>();
                var jobExecutor = x.GetRequiredService<JobExecutor>();

                return new StreamZeebeWorker<T>(client, serviceTaskConfiguration, jobExecutor, logger);
            });
        }
        
        _services.AddHostedService(x =>
        {
            var client = x.GetRequiredService<Gateway.GatewayClient>();
            var logger = x.GetRequiredService<ILogger<PoolZeebeWorker<T>>>();
            var jobExecutor = x.GetRequiredService<JobExecutor>();

            return new PoolZeebeWorker<T>(client, serviceTaskConfiguration, jobExecutor, logger);
        });

        return this;
    }

    public IZeebeBuilder Configure(Action<IServiceCollection> configure = null)
    {
        configure?.Invoke(_services);
        return this;
    }
}