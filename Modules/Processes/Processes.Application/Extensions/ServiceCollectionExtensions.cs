using Camunda.Client;
using Common.Application.Extensions;
using MassTransit;
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

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumer<SimulationFinishedEventHandler>();
            x.AddConsumer<DecisionGeneratedEventHandler>();
            x.AddConsumer<ContractSignedEventHandler>();
            x.AddConsumer<ApplicationClosedEventHandler>();
            x.AddConsumer<ApplicationRegisteredEventHandler>();

            if (configuration.IsKafka())
            {
                x.AddRider(configure =>
                {
                    configure.UsingKafka((context, cfg) =>
                    {
                        cfg.Host(configuration.GetkafkaConnectionString());
                        cfg.TopicEndpoint<SimulationFinished>("event.credit.calculations.simulationFinished.v1", configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<SimulationFinishedEventHandler>(context);
                        });

                        cfg.TopicEndpoint<DecisionGenerated>("command.credit.applications.decision.v1", configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<DecisionGeneratedEventHandler>(context);
                        });

                        cfg.TopicEndpoint<ContractSigned>("event.credit.applications.decisionGenerated.v1", configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<ContractSignedEventHandler>(context);
                        });

                        cfg.TopicEndpoint<ApplicationClosed>("event.credit.applications.applicationClosed.v1", configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<ApplicationClosedEventHandler>(context);
                        });

                        cfg.TopicEndpoint<ApplicationRegistered>("event.credit.applications.applicationRegistered.v1", configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<ApplicationRegisteredEventHandler>(context);
                        });
                    });
                });
            }
            else
            {
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetAzServiceBusConnectionString());

                    cfg.ConfigureEndpoints(context);
                });
            }
        });
    }
}
