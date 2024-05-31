using Common.Application.Extensions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application;

public class BusProxy(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task Publish<T>(T message, CancellationToken cancellationToken) where T : class
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        //if (!_configuration.IsKafka())
        //{
        //    var bus = serviceScope.ServiceProvider.GetRequiredService<IBus>();
        //    await bus.Publish(message, cancellationToken);
        //    return;
        //}

        var topicProducer = serviceScope.ServiceProvider.GetRequiredService<ITopicProducer<T>>();
        await topicProducer.Produce(message, cancellationToken);
    }
}
