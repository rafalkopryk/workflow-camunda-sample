using Applications.Application.Features.CloseApplication;
using Applications.Contracts.Commands;
using Calculations.Contracts;
using Common.Application.Extensions;
using Elsa.Workflows.Runtime;
using JasperFx.Resources;
using Processes.Elsa.WebApi.Features.CreditApplications;
using Wolverine;
using Wolverine.Kafka;

namespace Processes.Elsa.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureWolverine(this WolverineOptions options, IConfiguration configuration)
    {
        options.UseRuntimeCompilation();
        options.CodeGeneration.AlwaysUseServiceLocationFor<IWorkflowRuntime>();
        options.CodeGeneration.AlwaysUseServiceLocationFor<IWorkflowDispatcher>();
        options.CodeGeneration.AlwaysUseServiceLocationFor<IEventPublisher>();

        if (configuration.IsKafka())
        {
            options.UseKafka(configuration.GetkafkaConnectionString())
                .ConfigureConsumers(consumer => consumer = configuration.GetkafkaConsumer()!)
                .ConfigureProducers(producer => producer = configuration.GetkafkaProducer()!);

            options.PublishMessage<CloseApplicationCommand>().ToKafkaTopic("applications").TelemetryEnabled(true);
            options.PublishMessage<SimulateCreditCommand>().ToKafkaTopic("simulations").TelemetryEnabled(true);
            options.PublishMessage<SetDecisionCommand>().ToKafkaTopic("decisions").TelemetryEnabled(true);
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
