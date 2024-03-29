﻿using GatewayProtocol;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Client;

public static class ServiceCollectionExtensions
{
    public static void AddZeebe(
        this IServiceCollection services,
        Action<ZeebeOptions> options,
        Action<ZeebeBuilder>? configure = null)
    {
        var zeebeOptions = new ZeebeOptions();
        options?.Invoke(zeebeOptions);

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
            client.Address = new Uri(zeebeOptions.Endpoint);
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

        var zeebeBuilder = new ZeebeBuilder(services);
        configure?.Invoke(zeebeBuilder);

        services.AddSingleton<IJobClient, JobClient>();
        services.AddSingleton<IMessageClient, MessageClient>();
        services.AddSingleton<JobExecutor>();
    }
}
