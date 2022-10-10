namespace Common.Application.Extensions;

using Common.Application.Zeebe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IZeebeService, ZeebeService>();

        var zeebejobProvider = new ZeebeJobHandlerProvider();
        zeebejobProvider.RegisterZeebeJobs();

        services.AddSingleton<IZeebeJobHandlerProvider>(zeebejobProvider);

        services.AddHostedService<ZeebeWorker>();
    }
}

