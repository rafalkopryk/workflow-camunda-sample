using JasperFx.Resources;

namespace Applications.Application.Extensions;

using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.CloseApplication;
using Applications.Application.UseCases.SetDecision;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Application.Extensions;
using Applications.Application.UseCases.RegisterApplication;
using Applications.Application.UseCases.SignContract;
using Wolverine;
using Wolverine.Kafka;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Wolverine.AzureServiceBus;
using Common.Application.Cqrs;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.IsCosmosDb())
        {
            services.AddDbContextPool<CreditApplicationDbContext>(options => options.UseCosmos(configuration.GetAzCosmosDBConnectionString(), "Credit_Applications"));
        }
        else if (configuration.IsMongoDb())
        {
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(configuration.GetMongoDbConnectionString()));
            var options = new InstrumentationOptions { CaptureCommandText = true };
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber(options));
            var mongoClient = new MongoClient(clientSettings);

            services.AddDbContextPool<CreditApplicationDbContext>(options => options.UseMongoDB(mongoClient, "Credit_Applications"));
        }
        else
        {
            services.AddDbContextPool<CreditApplicationDbContext>(options => options.UseSqlServer(configuration.GetSqlConnectionString(), b => b.MigrationsAssembly("Applications.WebApi")));
        }
        
        services.RegisterHandlersFromAssemblies(typeof(ServiceCollectionExtensions).Assembly);
    }

    public static void ConfigureWolverine(this WolverineOptions opts, IConfiguration configuration)
    {
        if (configuration.IsKafka())
        {
            opts.UseKafka(configuration.GetkafkaConnectionString())
                .ConfigureConsumers(consumer => consumer = configuration.GetkafkaConsumer())
                .ConfigureProducers(producer => producer = configuration.GetkafkaProducer());

            opts.PublishMessage<ApplicationRegistered>().ToKafkaTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<ApplicationRegisteredFast>().ToKafkaTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<ApplicationClosed>().ToKafkaTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<DecisionGenerated>().ToKafkaTopic("decisions").TelemetryEnabled(true);
            opts.PublishMessage<ContractSigned>().ToKafkaTopic("contracts").TelemetryEnabled(true);

            opts.ListenToKafkaTopic("applications")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToKafkaTopic("decisions")
                .ProcessInline().TelemetryEnabled(true);

            opts.Services.AddResourceSetupOnStartup();
        }
        else
        {
            opts.UseAzureServiceBus(configuration.GetAzServiceBusConnectionString())
                .AutoProvision();

            opts.PublishMessage<ApplicationRegisteredFast>().ToAzureServiceBusTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<ApplicationClosed>().ToAzureServiceBusTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<DecisionGenerated>().ToAzureServiceBusTopic("decisions").TelemetryEnabled(true);
            opts.PublishMessage<ContractSigned>().ToAzureServiceBusTopic("contracts").TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("applications-applications-subs")
                .FromTopic("applications")
                .ProcessInline().TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("decisions-applications-subs")
                .FromTopic("decisions")
                .ProcessInline().TelemetryEnabled(true);
        }

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }

    public static async Task ConfigureApplication(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider.GetRequiredService<CreditApplicationDbContext>();
        //if (context.Database.GetPendingMigrations().Any())
        //{
        //    context.Database.Migrate();
        //}
        await context.Database.EnsureCreatedAsync();
    }
}

