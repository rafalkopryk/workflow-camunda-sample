using Camunda.Client;
using Common.Application.Extensions;
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

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}