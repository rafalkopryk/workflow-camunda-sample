using Confluent.Kafka;
using MassTransit;
using System.Reflection;

namespace Processes.Application.Extensions;

public static class KafkaMassTransitExtensions
{
    public static void AddProducer<TKey, T>(this IRiderRegistrationConfigurator configurator) where T : class
    {
        var entity = typeof(T).GetCustomAttributes<EntityNameAttribute>().FirstOrDefault();
        configurator.AddProducer<T>(entity.EntityName);
    }

    public static void TopicEndpoint<T>(this IKafkaFactoryConfigurator configurator, ConsumerConfig consumerConfig,
            Action<IKafkaTopicReceiveEndpointConfigurator<Ignore, T>> configure)
            where T : class
    {
        var entity = typeof(T).GetCustomAttributes<EntityNameAttribute>().FirstOrDefault();
        configurator.TopicEndpoint(entity.EntityName, consumerConfig, configure);
    }
}

