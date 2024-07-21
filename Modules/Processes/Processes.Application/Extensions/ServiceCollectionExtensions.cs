using Camunda.Client;
using Common.Application.Extensions;
using JasperFx.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oakton.Resources;
using Processes.Application.UseCases.CreditApplications.Close;
using Processes.Application.UseCases.CreditApplications.Contract;
using Processes.Application.UseCases.CreditApplications.Decision;
using Processes.Application.UseCases.CreditApplications.Simulation;
using Processes.Application.Utils;
using Processes.Application.Utils.Importer.File;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.AzureServiceBus;
using Wolverine.Kafka;

namespace Processes.Application.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly));

        services.Configure<ProcessDefinitionOptions>(configuration.GetSection("ProcessDefinitionsOptions"));
        services.Configure<PathFileProviderOptions>(configuration.GetSection("PathDefinitionsOptions"));

        services.AddSingleton<IBpmnProvider, PathFileProvider>();
        services.AddHostedService<DeployBPMNDefinitionService>();

        services.AddZeebe(
            options => configuration.GetSection("Zeebe").Bind(options),
            builder => builder
                .AddWorker<SimulationJobHandler>()
                .AddWorker<DecisionJobHandler>()
                .AddWorker<CloseApplicationJobHandler>());
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

            opts.ListenToKafkaTopic("applications")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("simulations")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("contracts")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("decisions")
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
        }

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}