using Camunda.Connector.Kafka.Outbound.Model;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Camunda.Connector.Kafka.Outbound;

[OutboundConnector(
    Name = "KAFKA",
    InputVariables = new[] { "authentication", "topic", "message", "additionalProperties" },
    Type = "io.camunda:connector-kafka:1")]
public class KafkaConnectorFunction : IOutboundConnectorFunction
{
    private readonly Lazy<IProducer<string, string>> _producer;

    public KafkaConnectorFunction(IOptions<ProducerConfig> producerConfig)
    {
        _producer = new Lazy<IProducer<string, string>>(new ProducerBuilder<string, string>(producerConfig.Value).Build());
    }

    public async Task<object> Execute(IOutboundConnectorContext context)
    {
        var producer = _producer.Value;

        var connectorRequest = context.GetVariablesAsType<KafkaConnectorRequest>();

        var data = JsonSerializer.Serialize(connectorRequest.Message.Value, JsonSerializerKafkaOptions.CamelCase);
        var message = new Message<string, string>
        {
            Key = connectorRequest.Message.Key.ToString(),
            Value = data,
            Headers = Activity.Current?.Id != null
             ? new Headers
             {
                 new Header("traceparent", Encoding.ASCII.GetBytes(Activity.Current.Id)),
             }
            : new Headers()
        };

        using var activity = Diagnostics.Producer.Start(connectorRequest.Topic.TopicName, message);
        activity?.AddDefaultOpenTelemetryTags(connectorRequest.Topic.TopicName, message);

        var result = await producer.ProduceAsync(connectorRequest.Topic.TopicName, message, CancellationToken.None);
        return new KafkaConnectorResponse(result.Topic, result.Timestamp.UnixTimestampMs, result.Offset, result.Partition.Value);
    }
}
