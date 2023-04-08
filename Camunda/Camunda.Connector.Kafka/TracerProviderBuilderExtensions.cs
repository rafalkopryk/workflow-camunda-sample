using OpenTelemetry.Trace;

namespace Camunda.Connector.Kafka;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddKafkaConnectorInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddSource(Diagnostics.ActivitySource.Name);
    }
}
