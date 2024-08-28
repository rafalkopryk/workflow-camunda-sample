using Camunda.Client.Jobs;
using Camunda.Client.Rest;
using GatewayProtocol;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Client;

public static class ServiceCollectionExtensions
{
    public static void AddCamunda(
        this IServiceCollection services,
        Action<CamundaOptions> options,
        Action<CamundaBuilder>? configure = null)
    {
        var camundaOptions = new CamundaOptions();
        options?.Invoke(camundaOptions);

        ConfigureCamundaGrpc(services, configure, camundaOptions.CamundaGrpc);
        ConfigureCamundaRest(services, configure, camundaOptions.CamundaRest);

        services.AddSingleton<JobExecutor>();
        services.AddSingleton<IMessageClient, GrpcMessageClient>();

        if (camundaOptions.UseRest)
        {
            services.AddSingleton<IJobClient, RestJobClient>();
        }
        else
        {
            services.AddSingleton<IJobClient, GrpcJobClient>();
        }

        var camundaBuilder = new CamundaBuilder(services, camundaOptions.UseRest);
        configure?.Invoke(camundaBuilder);
    }

    private static void ConfigureCamundaGrpc(IServiceCollection services, Action<CamundaBuilder>? configure, GrpcCamundaOptions camundaOptions)
    {
        var defaultMethodConfig = new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 3,
                InitialBackoff = TimeSpan.FromMilliseconds(500),
                MaxBackoff = TimeSpan.FromSeconds(1),
                BackoffMultiplier = 1.0,
                RetryableStatusCodes = { StatusCode.Unavailable, StatusCode.ResourceExhausted }
            }
        };
        services.AddGrpcClient<Gateway.GatewayClient>(client =>
        {
            client.Address = new Uri(camundaOptions!.Endpoint);
        })
        .ConfigureChannel(configureChannel =>
        {
            configureChannel.ServiceConfig = new ServiceConfig
            {
                MethodConfigs = { defaultMethodConfig }
            };
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            };
        });
    }

    private static void ConfigureCamundaRest(IServiceCollection services, Action<CamundaBuilder>? configure, RestCamundaOptions camundaOptions)
    {
        services.AddHttpClient<CamundaClientRest>(client =>
        {
            client.BaseAddress = new Uri(camundaOptions!.Endpoint);
        });
    }
}
