using System.Diagnostics;
using Camunda.Orchestration.Sdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Camunda.Client.Extensions;

public static class CamundaWorkerExtensions
{
    public static IHostApplicationBuilder AddCamundaWorkers(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHostedService<CamundaWorkerHostedService>();
        return builder;
    }

    public static IHost CreateJobWorker<T>(
        this IHost host,
        JobWorkerConfig config) where T : class, IJobHandler
    {
        host.GetCamundaClient().CreateJobWorker<T>(config, host.Services);
        return host;
    }

    public static IHost CreateJobWorker<T, TOutput>(
        this IHost host,
        JobWorkerConfig config) where T : class, IJobHandler<TOutput>
    {
        host.GetCamundaClient().CreateJobWorker<T, TOutput>(config, host.Services);
        return host;
    }

    public static IHost CreateJobWorker(
        this IHost host,
        JobWorkerConfig config,
        Func<ActivatedJob, CancellationToken, Task> handler)
    {
        host.GetCamundaClient().CreateJobWorker(config, async (job, ct) =>
        {
            await CamundaClientExtensions.WithTracing(job, () => handler(job, ct));
        });
        return host;
    }

    private static CamundaClient GetCamundaClient(this IHost host) =>
        host.Services.GetRequiredService<CamundaClient>();
}

public static class CamundaClientExtensions
{
    public static CamundaClient CreateJobWorker<T>(
        this CamundaClient client,
        JobWorkerConfig config,
        IServiceProvider serviceProvider) where T : class, IJobHandler
    {
        client.CreateJobWorker(config, async (job, ct) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var handler = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);
            await WithTracing(job, () => handler.HandleAsync(job, ct));
        });

        return client;
    }

    public static CamundaClient CreateJobWorker<T, TOutput>(
        this CamundaClient client,
        JobWorkerConfig config,
        IServiceProvider serviceProvider) where T : class, IJobHandler<TOutput>
    {
        client.CreateJobWorker(config, async (job, ct) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var handler = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);
            return await WithTracing(job, () => handler.HandleAsync(job, ct));
        });

        return client;
    }

    internal static async Task WithTracing(ActivatedJob job, Func<Task> action)
    {
        using var activity = Diagnostics.StartActivity(job);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    private static async Task<T> WithTracing<T>(ActivatedJob job, Func<Task<T>> action)
    {
        using var activity = Diagnostics.StartActivity(job);
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}

public class CamundaWorkerHostedService(
    CamundaClient client,
    ILogger<CamundaWorkerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Camunda workers");
        await client.RunWorkersAsync(ct: stoppingToken);
    }
}
