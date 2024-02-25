using Camunda.Client;
using Common.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Processes.Application.UseCases.CreditApplications.Close;
using Processes.Application.UseCases.CreditApplications.Contract;
using Processes.Application.UseCases.CreditApplications.Decision;
using Processes.Application.UseCases.CreditApplications.Simulation;
using Processes.Application.Utils;
using Processes.Application.Utils.Importer.File;

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

        services.AddKafka(
           options => configuration.GetSection("EventBus").Bind(options),
           options => configuration.GetSection("EventBus").Bind(options),
           builder => builder
               .UseTopic<SimulationFinished>()
               .UseTopic<DecisionGenerated>()
               .UseTopic<ContractSigned>()
               .UseTopic<ApplicationClosed>());
    }
}
