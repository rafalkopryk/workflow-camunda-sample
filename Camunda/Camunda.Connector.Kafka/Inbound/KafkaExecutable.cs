using Camunda.Connector.Kafka.Inbound.Model;
using Camunda.Connector.SDK.Core.Api.Annotation;
using Camunda.Connector.SDK.Core.Api.Inbound;
using System.Text.Json;

namespace Camunda.Connector.Kafka.Inbound;

[InboundConnector(
    Name = "KAFKA",
    Type = "io.camunda:connector-kafka-inbound:1")]
internal class KafkaExecutable : IInboundConnectorExecutable
{
    private readonly IKafkaSubscription _subscription;

    public KafkaExecutable(IKafkaSubscription subscription)
    {
        _subscription = subscription;
    }

    public void Deactivate()
    {
        //TODO
        //_consumer.Close();
    }

    public async Task Activate(IInboundConnectorContext context, CancellationToken cancellationToken)
    {
        var properties = context.GetPropertiesAsType<KafkaConnectorProperties>();
        await _subscription.Subscribe(
            properties,
            context.Correlate,
            cancellationToken);
    }
}
