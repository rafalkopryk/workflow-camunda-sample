using Camunda.Connector.Kafka.Outbound.Model;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Confluent.Kafka;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Camunda.Connector.Kafka.Model;

[OutboundConnector(
    Name = "KAFKA",
    InputVariables = new[] { "authentication", "topic", "message", "additionalProperties" },
    Type = "io.camunda:connector-kafka:1")]
public class KafkaConnectorFunction : IOutboundConnectorFunction
{
    private readonly IProducer<string, string> _producer;

    public KafkaConnectorFunction(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task<object> Execute(IOutboundConnectorContext context)
    {
        var connectorRequest = context.GetVariablesAsType<KafkaConnectorRequest>();

        var data = JsonSerializer.Serialize(connectorRequest.Message.Value);
        var message = new Message<string, string>
        {
            Key = connectorRequest.Message.Key.ToString(),
            Value = data,
            //Headers = new Headers
            //{
            //    new Header("traceparent", Encoding.ASCII.GetBytes(Activity.Current.Id)),
            //}
        };
        
        var result = await _producer.ProduceAsync(connectorRequest.Topic.TopicName, message, CancellationToken.None);
        return new KafkaConnectorResponse(result.Topic, result.Timestamp.UnixTimestampMs, result.Offset, result.Partition.Value);
    }
}
