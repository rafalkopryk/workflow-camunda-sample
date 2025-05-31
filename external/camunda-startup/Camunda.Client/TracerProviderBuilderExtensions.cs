using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Camunda.Client;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddZeebeWorkerInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddSource(Diagnostics.ActivitySource.Name);
    }

    public static MeterProviderBuilder AddZeebeWorkerInstrumentation(this MeterProviderBuilder builder)
    {
        return builder.AddMeter(Diagnostics.Meter.Name);
    }
}
