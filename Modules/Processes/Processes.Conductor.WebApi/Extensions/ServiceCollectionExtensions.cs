using Applications.Contracts.Commands;
using Calculations.Contracts;
using Common.Application.Extensions;
using JasperFx.Resources;
using Wolverine;
using Wolverine.Kafka;

namespace Processes.Conductor.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureWolverine(this WolverineOptions options, IConfiguration configuration)
    {
        options.UseRuntimeCompilation();

        if (configuration.IsKafka())
        {
            options.UseKafka(configuration.GetkafkaConnectionString())
                .ConfigureConsumers(consumer => consumer = configuration.GetkafkaConsumer()!)
                .ConfigureProducers(producer => producer = configuration.GetkafkaProducer()!);

            options.PublishMessage<CloseApplicationCommand>().ToKafkaTopic("applications").TelemetryEnabled(true);
            options.PublishMessage<SimulationCommand>().ToKafkaTopic("simulations").TelemetryEnabled(true);
            options.PublishMessage<DecisionCommand>().ToKafkaTopic("decisions").TelemetryEnabled(true);
            options.PublishMessage<CustomerVerificationCommand>().ToKafkaTopic("customer-verifications").TelemetryEnabled(true);

            options.ListenToKafkaTopic("applications").ProcessInline().TelemetryEnabled(true);
            options.ListenToKafkaTopic("simulations").ProcessInline().TelemetryEnabled(true);
            options.ListenToKafkaTopic("contracts").ProcessInline().TelemetryEnabled(true);
            options.ListenToKafkaTopic("decisions").ProcessInline().TelemetryEnabled(true);
            options.ListenToKafkaTopic("customer-verifications").ProcessInline().TelemetryEnabled(true);

            options.Services.AddResourceSetupOnStartup();
        }

        options.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}
