using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Client;

public class ZeebeBuilder
{
    //TODO change to private
    private readonly IServiceCollection _services;

    public ZeebeBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ZeebeBuilder AddWorker<T>() where T : IJobHandler
    {
        var attribute = typeof(T).GetAttribute<ZeebeWorkerAttribute>();
        _services.AddSingleton(new ZeebeConfiguration(typeof(T), attribute));
        _services.AddScoped(typeof(T));
        _services.AddHostedService<ZeebeWorker<T>>();
        return this;
    }

    public ZeebeBuilder AddWorker<T>(ZeebeWorkerAttribute attribute) where T : IJobHandler
    {
        _services.AddSingleton(new ZeebeConfiguration(typeof(T), attribute));
        _services.AddScoped(typeof(T));
        _services.AddHostedService<ZeebeWorker<T>>();
        return this;
    }
}

public record ZeebeConfiguration(Type Type, ZeebeWorkerAttribute Attribute);