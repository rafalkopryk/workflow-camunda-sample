using Common.Application.Zeebe;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Zeebe;

public class ZeebeBuilder
{
    private readonly IServiceCollection _services;

    public ZeebeBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ZeebeBuilder AddService()
    {
        _services.AddSingleton<IZeebeService, ZeebeService>();
        return this;
    }

    public ZeebeBuilder AddTaskWorker<T>() where T : IZeebeTask
    {
        _services.AddHostedService<ZeebeWorker<T>>();
        return this;
    }
}