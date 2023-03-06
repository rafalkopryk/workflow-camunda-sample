using Camunda.Connector.Kafka.Inbound.Model;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Camunda.Connector.Kafka.Inbound;

[InboundConnector(
    Name = "KAFKA",
    Type = "io.camunda:connector-kafka:1")]
internal class KafkaExecutable : IInboundConnectorExecutable
{
    private readonly ILogger _logger;
    private readonly ConsumerConfig _consumerConfig;

    public KafkaExecutable(ILogger<KafkaExecutable> logger, IOptions<ConsumerConfig> consumerConfig)
    {
        _logger = logger;
        _consumerConfig = consumerConfig.Value;
    }

    public void Deactivate()
    {
        //TODO
        //_consumer.Close();
    }

    public async Task Activate(IInboundConnectorContext context, CancellationToken cancellationToken)
    {
        var properties = context.GetPropertiesAsType<KafkaProperties>();


        //TODO fix it
        Task.Run(async () =>
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
            consumer.Subscribe(properties.Topic);
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    await ConsumeNextEvent(context, consumer, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error consuming message");
                //_consumer.Close();
            }
        }, cancellationToken);

        await Task.CompletedTask;
    }

    private async Task ConsumeNextEvent(IInboundConnectorContext context, IConsumer<Ignore, string> consumer, CancellationToken cancellationToken)
    {
        try
        {
            var message = consumer.Consume(cancellationToken);
            if (message.Message == null) return;

            try
            {
                var body = message.Message.Value;
                var result = await context.Correlate(body);

                //TODO
                consumer.Commit();
            }
            catch (Exception e)
            {
                throw;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error consuming message");
            //_consumer.Commit();
        }
    }
}
