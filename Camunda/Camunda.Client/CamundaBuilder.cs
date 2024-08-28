using Camunda.Client.Jobs;
using Camunda.Client.Rest;
using Camunda.Client.Workers;
using GatewayProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Camunda.Client;

public class CamundaBuilder(IServiceCollection services, bool useRest) : ICamundaBuilder
{
    private readonly IServiceCollection _services = services;
    private readonly bool _useRest = useRest;

    public ICamundaBuilder AddWorker<T>() where T : class, IJobHandler
    {
        var attribute = typeof(T).GetAttribute<JobWorkerAttribute>();
        var serviceTaskConfiguration = new JobWorkerConfiguration
        {
            Type = attribute.Type,
            AutoComplete = attribute.AutoComplete,
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

    public ICamundaBuilder AddWorker<T>(JobWorkerConfiguration serviceTaskConfiguration) where T : class, IJobHandler
    {
        _services.AddScoped(typeof(T));

        if (serviceTaskConfiguration.UseStream)
        {
            _services.AddHostedService(x =>
            {
                var client = x.GetRequiredService<Gateway.GatewayClient>();
                var logger = x.GetRequiredService<ILogger<GrpcStreamCamundaWorker<T>>>();
                var jobExecutor = x.GetRequiredService<JobExecutor>();

                return new GrpcStreamCamundaWorker<T>(client, serviceTaskConfiguration, jobExecutor, logger);
            });
        }

        if (_useRest)
        {
            _services.AddHostedService(x =>
            {
                var client = x.GetRequiredService<CamundaClientRest>();
                var logger = x.GetRequiredService<ILogger<GrpcPoolCamundaWorker<T>>>();
                var jobExecutor = x.GetRequiredService<JobExecutor>();

                return new RestPoolCamundaWorker<T>(client, serviceTaskConfiguration, jobExecutor, logger);
            });
        }
        else
        {
            _services.AddHostedService(x =>
            {
                var client = x.GetRequiredService<Gateway.GatewayClient>();
                var logger = x.GetRequiredService<ILogger<GrpcPoolCamundaWorker<T>>>();
                var jobExecutor = x.GetRequiredService<JobExecutor>();

                return new GrpcPoolCamundaWorker<T>(client, serviceTaskConfiguration, jobExecutor, logger);
            });
        }

        return this;
    }
}