using Common.Application.Zeebe;
using GatewayProtocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Zeebe;

public static class ServiceCollectionExtensions
{
    public static void AddZeebeGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ZeebeOptions>(options => configuration.GetSection("ZEEBE").Bind(options));
        var uri = new Uri(configuration["ZEEBE:ADDRESS"]);

        services.AddGrpcClient<Gateway.GatewayClient>(client =>
        {
            client.Address = uri;
        })
        .ConfigureChannel(configureChannel =>
        {
            configureChannel.MaxRetryAttempts = 3;
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

        services.AddSingleton<IZeebeService, ZeebeService>();

        var zeebejobProvider = new ZeebeJobHandlerProvider();
        zeebejobProvider.RegisterZeebeJobs();

        services.AddSingleton<IZeebeJobHandlerProvider>(zeebejobProvider);

        services.AddHostedService<ZeebeWorker>();
    }
}
