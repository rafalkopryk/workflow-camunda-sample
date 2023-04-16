using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Kafka;

public class KafkaBuilder
{
    private readonly IServiceCollection _services;

    public KafkaBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public KafkaBuilder UseTopic<T>() where T : INotification
    {
        _services.AddHostedService<KafkaWorker<T>>();
        return this;
    }
}