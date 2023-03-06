namespace Common.Kafka;

using MediatR;
using System.Threading;
using System.Threading.Tasks;

public interface IEventBusProducer
{
    Task PublishAsync<TMessage>(TMessage @event, CancellationToken cancellationToken) where TMessage : INotification;
}
