
using Camunda.Client;
using OpenTelemetry.Trace;

namespace Camunda.Connector.Kafka;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddZeebeWorkerInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddSource(Diagnostics.ActivitySource.Name);
    }
}
