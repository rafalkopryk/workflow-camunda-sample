using Camunda.Client.Jobs;
using Camunda.Client.Rest;
using Camunda.Client.Workers;
using GatewayProtocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Camunda.Client;

public class CamundaBuilder(IServiceCollection services) : ICamundaBuilder
{
    private readonly IServiceCollection _services = services;

    public ICamundaBuilder AddWorker<T>(JobWorkerConfiguration jobWorkerConfiguration) where T : class, IJobHandler
    {
        var attribute = typeof(T).GetAttribute<JobWorkerAttribute>();
        AddWorker<T>(attribute.Type, jobWorkerConfiguration, attribute.FetchVariabeles);
        return this;
    }

    public ICamundaBuilder AddWorker<T>(string type, JobWorkerConfiguration jobWorkerConfiguration, string[]? fetchVariabeles  = null) where T : class, IJobHandler
    {
        _services.AddScoped(typeof(T));

        var internalJobWorkerConfiguration = new InternalJobWorkerConfiguration
        {
            Type = type,
            FetchVariabeles = fetchVariabeles ?? [],
            AutoComplete = jobWorkerConfiguration.AutoComplete,
            PoolingDelayInMs = jobWorkerConfiguration.PoolingDelayInMs,
            PoolingMaxJobsToActivate = jobWorkerConfiguration.PoolingMaxJobsToActivate,
            PoolingRequestTimeoutInMs = jobWorkerConfiguration.PoolingRequestTimeoutInMs,
            RetryBackOffInMs = jobWorkerConfiguration.RetryBackOffInMs,
            StreamTimeoutInSec = jobWorkerConfiguration.StreamTimeoutInSec,
            TenatIds = jobWorkerConfiguration.TenatIds,
            TimeoutInMs = jobWorkerConfiguration.TimeoutInMs,
            UseStream = jobWorkerConfiguration.UseStream,
        };

        if (jobWorkerConfiguration.UseStream)
        {
            _services.AddHostedService(x =>
            {
                var client = x.GetRequiredService<Gateway.GatewayClient>();
                var logger = x.GetRequiredService<ILogger<GrpcStreamCamundaWorker<T>>>();
                var jobExecutor = x.GetRequiredService<JobExecutor>();

                return new GrpcStreamCamundaWorker<T>(client, internalJobWorkerConfiguration, jobExecutor, logger);
            });

            return this;
        }

        _services.AddHostedService(x =>
        {
            var client = x.GetRequiredService<ICamundaClientRest>();
            var logger = x.GetRequiredService<ILogger<RestPoolCamundaWorker<T>>>();
            var jobExecutor = x.GetRequiredService<JobExecutor>();

            return new RestPoolCamundaWorker<T>(client, internalJobWorkerConfiguration, jobExecutor, logger);
        });

        return this;
    }
}