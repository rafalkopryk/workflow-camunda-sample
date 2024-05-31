using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using CreditProcessFunctions.UseCases;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostBuilder, services) =>
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(hostBuilder.Configuration.GetValue<string>("AzServiceBusConnectionString"));

            foreach (var topic in TopicAttributes.Produces)
            {
                builder
                    .AddClient<ServiceBusSender, ServiceBusClientOptions>((_, _, provider) => provider.GetService<ServiceBusClient>().CreateSender(topic))
                    .WithName(topic);
            }
        });

        services.AddSingleton(new ManagementClient(hostBuilder.Configuration.GetValue<string>("AzServiceBusConnectionString")));
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

using var scope = host.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;

var client = serviceProvider.GetRequiredService<ManagementClient>();

foreach (var topic in TopicAttributes.Produces)
{
    await client.TryCreateTopic(topic);
}

foreach (var @event in TopicAttributes.Receives)
{
    await client.TryCreateTopic(@event.Topic);
    await client.TryCreateSubcription(@event.Topic, @event.Subcription);
}

host.Run();
