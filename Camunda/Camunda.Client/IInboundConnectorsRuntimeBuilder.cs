using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Client;

public interface IZeebeBuilder
{
    IZeebeBuilder AddWorker<T>(ServiceTaskConfiguration serviceTaskConfiguration, Action<IServiceCollection> configure = null) where T : class, IJobHandler;

    IZeebeBuilder Configure(Action<IServiceCollection> configure = null);
}
