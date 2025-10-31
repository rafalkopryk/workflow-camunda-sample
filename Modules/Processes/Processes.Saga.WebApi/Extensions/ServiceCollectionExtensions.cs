using Common.Application.Extensions;
using JasperFx.Resources;
using Processes.Saga.WebApi.UseCases.CreditApplications;
using Wolverine;
using Wolverine.AzureServiceBus;
using Wolverine.Kafka;
using Wolverine.RDBMS;
using Wolverine.SqlServer;
using Wolverine.Configuration;
using Wolverine.Postgresql;


namespace Processes.Application.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
    }

    public static void ConfigureWolverine(this WolverineOptions opts, IConfiguration configuration)
    {
        //opts.AddSagaType<CreditApplicationFast>();
        opts.AddSagaType<CreditApplication>();

        if (configuration.IsPostgres())
        {
            opts.PersistMessagesWithPostgresql(configuration.GetPostgresConnectionString(), "creditapplication_sagas");
        }
        else
        {
            opts.PersistMessagesWithSqlServer(configuration.GetSqlConnectionString(), "creditapplication_sagas");
        }

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
        else
        {
            opts.UseAzureServiceBus(configuration.GetAzServiceBusConnectionString())
                .AutoProvision();

            opts.PublishMessage<CloseApplicationCommand>().ToAzureServiceBusTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<SimulationCommand>().ToAzureServiceBusTopic("simulations").TelemetryEnabled(true);
            opts.PublishMessage<DecisionCommand>().ToAzureServiceBusTopic("decisions").TelemetryEnabled(true);
            opts.PublishMessage<CustomerVerificationCommand>().ToAzureServiceBusTopic("customer-verifications").TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("applications-processes-subs")
                .FromTopic("applications")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("simulations-processes-subs")
                .FromTopic("simulations")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("contracts-processes-subs")
                .FromTopic("contracts")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("decisions-processes-subs")
                .FromTopic("decisions")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("customer-verifications-processes-subs")
                .FromTopic("customer-verifications")
                .ProcessInline().TelemetryEnabled(true);
        }

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}