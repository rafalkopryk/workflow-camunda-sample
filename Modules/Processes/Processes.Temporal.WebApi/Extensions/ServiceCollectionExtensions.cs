using Common.Application.Extensions;
using JasperFx.Resources;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Close;
using Processes.Temporal.WebApi.UseCases.CreditApplications.CustomerVerification;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Decision;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Simulation;
using Wolverine;
using Wolverine.Kafka;

namespace Processes.Temporal.WebApi.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
    }

    public static void ConfigureWolverine(this WolverineOptions opts, IConfiguration configuration)
    {
        if (configuration.IsKafka())
        {
            opts.UseKafka(configuration.GetkafkaConnectionString())
                .ConfigureConsumers(consumer => consumer = configuration.GetkafkaConsumer())
                .ConfigureProducers(producer => producer = configuration.GetkafkaProducer());

            opts.PublishMessage<CloseApplicationCommand>().ToKafkaTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<SimulationCommand>().ToKafkaTopic("simulations").TelemetryEnabled(true);
            opts.PublishMessage<DecisionCommand>().ToKafkaTopic("decisions").TelemetryEnabled(true);
            opts.PublishMessage<CustomerVerificationCommand>().ToKafkaTopic("customer-verifications").TelemetryEnabled(true);
        
            opts.ListenToKafkaTopic("applications")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("simulations")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("contracts")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("decisions")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("customer-verifications")
                .ProcessInline().TelemetryEnabled(true);

            opts.Services.AddResourceSetupOnStartup();
        }

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}