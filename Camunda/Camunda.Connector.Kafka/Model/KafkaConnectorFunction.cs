using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Outbound;

namespace Camunda.Connector.Kafka.Model;

[OutboundConnector(
    Name = "KAFKA",
    InputVariables = new[] { "authentication", "topic", "message", "additionalProperties" },
    Type = "io.camunda:connector-kafka:1")]
public class KafkaConnectorFunction : IOutboundConnectorFunction
{
    public async Task<object> Execute(IOutboundConnectorContext context)
    {
        var connectorRequest = context.GetVariablesAsType<KafkaConnectorRequest>();
        await Task.Yield();
        return new KafkaConnectorResponse(connectorRequest.Topic.TopicName, 1, 1, 1);
    }
}

public record KafkaConnectorResponse(
    string Topic,
    long Timestamp,
    long Offset,
    int partition);
