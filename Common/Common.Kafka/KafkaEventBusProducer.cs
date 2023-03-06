namespace Common.Kafka;

using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Logging;

internal class KafkaEventBusProducer : IEventBusProducer
{
    private readonly IProducer<string, string> _producer;

    private readonly ILogger<KafkaEventBusProducer> _logger;

    public KafkaEventBusProducer(IProducer<string, string> producer, ILogger<KafkaEventBusProducer> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task PublishAsync<TMessage>(TMessage @event, CancellationToken cancellationToken) where TMessage : INotification
    {
        var data = JsonSerializer.Serialize(@event, JsonSerializerCustomOptions.CamelCase);
        var eventEnvelope = typeof(TMessage).GetEventEnvelopeAttribute() ?? throw new ArgumentNullException(nameof(EventEnvelopeAttribute));

        var message = new Message<string, string>
        {
            Key = eventEnvelope.Topic,
            Value = data,
        };

        try
        {
            await _producer.ProduceAsync(eventEnvelope.Topic, message, cancellationToken);

            _logger.LogInformation("Kafka SEND to {Topic}", eventEnvelope.Topic);
        }
        catch (Exception e)
        {
            //activity?.RecordException(e);
            throw;
        }
    }
}
