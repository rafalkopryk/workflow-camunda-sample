using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Client;

public class ZeebeBuilder
{
    private readonly IServiceCollection _services;

    public ZeebeBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ZeebeBuilder AddWorker<T>() where T : IJobHandler
    {
        _services.AddScoped(typeof(T));
        _services.AddHostedService<ZeebeWorker<T>>();
        return this;
    }
}