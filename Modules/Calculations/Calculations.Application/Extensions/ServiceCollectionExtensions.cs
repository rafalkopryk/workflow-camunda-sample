using JasperFx.Resources;

namespace Calculations.Application.Extensions;

using Calculations.Application.Infrastructure.Database;
using Calculations.Application.UseCases.SimulateCreditCommand;
using Calculations.Application.UseCases.VerifyCustomerCommand;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Application.Extensions;
using Wolverine;
using Wolverine.Kafka;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MongoDB.Driver;
using Wolverine.AzureServiceBus;
using Common.Application.Cqrs;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.IsCosmosDb())
        {
            services.AddDbContextPool<CreditCalculationDbContext>(options => options.UseCosmos(configuration.GetAzCosmosDBConnectionString(), "Credit_Calculations"));
        }
        else if (configuration.IsMongoDb())
        {
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(configuration.GetMongoDbConnectionString()));
            var options = new InstrumentationOptions { CaptureCommandText = true };
            clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber(options));
            var mongoClient = new MongoClient(clientSettings);
            services.AddDbContextPool<CreditCalculationDbContext>(options => options.UseMongoDB(mongoClient, "Credit_Calculations"));
        }
        else
        {
            services.AddDbContextPool<CreditCalculationDbContext>(options => options.UseSqlServer(configuration.GetSqlConnectionString(), b => b.MigrationsAssembly("Calculations.WebApi")));
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

            opts.PublishMessage<SimulationCreditFinished>().ToKafkaTopic("simulations").TelemetryEnabled(true);
            opts.PublishMessage<CustomerVerified>().ToKafkaTopic("customer-verifications").TelemetryEnabled(true);

            opts.ListenToKafkaTopic("simulations")
                .ProcessInline().TelemetryEnabled(true);
            opts.ListenToKafkaTopic("customer-verifications")
                .ProcessInline().TelemetryEnabled(true);

            opts.Services.AddResourceSetupOnStartup();
        }
        else
        {
            opts.UseAzureServiceBus(configuration.GetAzServiceBusConnectionString())
                .AutoProvision();

            opts.PublishMessage<SimulationCreditFinished>().ToAzureServiceBusTopic("simulations").TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("simulations-calculations-subs")
                .FromTopic("simulations")
                .ProcessInline().TelemetryEnabled(true);
        }

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }

    public static async Task ConfigureApplication(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider.GetRequiredService<CreditCalculationDbContext>();
        //if (context.Database.GetPendingMigrations().Any())
        //{
        //    context.Database.Migrate();
        //}

        await context.Database.EnsureCreatedAsync();
    }
}

