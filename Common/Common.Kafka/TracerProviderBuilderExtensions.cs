using OpenTelemetry.Trace;

namespace Common.Kafka;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddKafkaInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddSource(Diagnostics.ActivitySource.Name);
    }
}
