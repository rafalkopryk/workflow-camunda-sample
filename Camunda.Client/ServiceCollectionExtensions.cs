using Camunda.Client.Jobs;
using Camunda.Client.Messages;
using Camunda.Client.Options;
using Camunda.Client.Rest;
using Camunda.Client.Workers;
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

        ConfigureCamundaGrpc(services, configure, camundaOptions);
        ConfigureCamundaRest(services, configure, camundaOptions);

        services.AddSingleton<JobExecutor>();
        services.AddSingleton<IJobClient, RestJobClient>();
        services.AddSingleton<IMessageClient, RestMessageClient>();

        var camundaBuilder = new CamundaBuilder(services);
        configure?.Invoke(camundaBuilder);
    }

    private static void ConfigureCamundaGrpc(IServiceCollection services, Action<CamundaBuilder>? configure, CamundaOptions camundaOptions)
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
            client.Address = new Uri(camundaOptions!.GrpcEndpoint);
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

    private static void ConfigureCamundaRest(IServiceCollection services, Action<CamundaBuilder>? configure, CamundaOptions camundaOptions)
    {
        services.AddHttpClient<ICamundaClientRest, CamundaClientRest>(client =>
        {
            client.BaseAddress = new Uri(camundaOptions!.Endpoint);
        });
        //.AddHttpMessageHandler(() => new LoggingHttpHandler());
    }
}

public class LoggingHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync();
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
        }

        return response;
    }
}
