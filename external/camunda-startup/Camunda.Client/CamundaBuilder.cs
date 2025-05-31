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
        AddWorker<T>(attribute.Type, jobWorkerConfiguration, attribute.FetchVariables);
        return this;
    }

    public ICamundaBuilder AddWorker<T>(string type, JobWorkerConfiguration jobWorkerConfiguration, string[]? fetchVariables  = null) where T : class, IJobHandler
    {
        _services.AddScoped(typeof(T));

        var internalJobWorkerConfiguration = new InternalJobWorkerConfiguration
        {
            Type = type,
            FetchVariables = fetchVariables ?? [],
            AutoComplete = jobWorkerConfiguration.AutoComplete,
            PollingDelayInMs = jobWorkerConfiguration.PollingDelayInMs,
            PollingMaxJobsToActivate = jobWorkerConfiguration.PollingMaxJobsToActivate,
            PollingRequestTimeoutInMs = jobWorkerConfiguration.PollingRequestTimeoutInMs,
            RetryBackOffInMs = jobWorkerConfiguration.RetryBackOffInMs,
            StreamTimeoutInSec = jobWorkerConfiguration.StreamTimeoutInSec,
            TenantIds = jobWorkerConfiguration.TenantIds,
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