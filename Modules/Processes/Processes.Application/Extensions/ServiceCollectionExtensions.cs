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
            if (configuration.IsKafka())
            {
                x.UsingInMemory();

                x.AddRider(rider =>
                {
                    rider.AddProducer<string, SimulationCommand>();
                    rider.AddProducer<string, DecisionCommand>();
                    rider.AddProducer<string, CloseApplicationCommand>();

                    rider.AddConsumer<SimulationFinishedEventHandler>();
                    rider.AddConsumer<DecisionGeneratedEventHandler>();
                    rider.AddConsumer<ContractSignedEventHandler>();
                    rider.AddConsumer<ApplicationClosedEventHandler>();
                    rider.AddConsumer<ApplicationRegisteredEventHandler>();

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host(configuration.GetkafkaConnectionString());
                        k.TopicEndpoint<SimulationFinished>(configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<SimulationFinishedEventHandler>(context);
                        });

                        k.TopicEndpoint<DecisionGenerated>(configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<DecisionGeneratedEventHandler>(context);
                        });

                        k.TopicEndpoint<ContractSigned>(configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<ContractSignedEventHandler>(context);
                        });

                        k.TopicEndpoint<ApplicationClosed>(configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<ApplicationClosedEventHandler>(context);
                        });

                        k.TopicEndpoint<ApplicationRegistered>(configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            //e.Handler<ApplicationRegistered>(async data =>
                            //{
                            //    var client = context.GetMessageClient();
                            //    await client.Publish(null, data.Message, data.Message.ApplicationId);
                            //});

                            e.ConfigureConsumer<ApplicationRegisteredEventHandler>(context);
                        });
                    });
                });
            }
            else
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<SimulationFinishedEventHandler>();
                x.AddConsumer<DecisionGeneratedEventHandler>();
                x.AddConsumer<ContractSignedEventHandler>();
                x.AddConsumer<ApplicationClosedEventHandler>();
                x.AddConsumer<ApplicationRegisteredEventHandler>();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration.GetAzServiceBusConnectionString());

                    cfg.ConfigureEndpoints(context);
                });
            }
        });
    }
}




public static class ServiceScopeProviderExtensions
{
    public static IMessageClient GetMessageClient(this IServiceProvider provider)
    {
        return provider.GetRequiredService<IMessageClient>();
    }
}