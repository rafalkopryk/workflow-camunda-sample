using Camunda.Connector.Kafka.Outbound.Model;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Camunda.Connector.Kafka.Outbound;

[OutboundConnector(
    Name = "KAFKA",
    InputVariables = new[] { "authentication", "topic", "message", "additionalProperties" },
    Type = "io.camunda:connector-kafka:1")]
public class KafkaConnectorFunction : IOutboundConnectorFunction
{
    private readonly ProducerConfig _producderConfig;

    public KafkaConnectorFunction(IOptions<ProducerConfig> producerConfig)
    {
        _producderConfig = producerConfig.Value;
    }

    public async Task<object> Execute(IOutboundConnectorContext context)
    {
        using var producer = new ProducerBuilder<string, string>(_producderConfig).Build();

        var connectorRequest = context.GetVariablesAsType<KafkaConnectorRequest>();

        var data = JsonSerializer.Serialize(connectorRequest.Message.Value, JsonSerializerKafkaOptions.CamelCase);
        var message = new Message<string, string>
        {
            Key = connectorRequest.Message.Key.ToString(),
            Value = data,
        };

        var result = await producer.ProduceAsync(connectorRequest.Topic.TopicName, message, CancellationToken.None);
        return new KafkaConnectorResponse(result.Topic, result.Timestamp.UnixTimestampMs, result.Offset, result.Partition.Value);
    }
}
