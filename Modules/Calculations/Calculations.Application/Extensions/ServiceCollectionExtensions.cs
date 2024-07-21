namespace Calculations.Application.Extensions;

using Calculations.Application.Infrastructure.Database;
using Calculations.Application.UseCases.SimulateCreditCommand;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Application.Extensions;
using Wolverine;
using Wolverine.Kafka;
using Oakton.Resources;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using MongoDB.Driver;

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

        services.AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly));

        //services.AddMassTransit(x =>
        //{
        //    x.SetKebabCaseEndpointNameFormatter();

        //    if (configuration.IsKafka())
        //    {
        //        x.AddRider(rider =>
        //        {
        //            rider.AddProducer<string, SimulationCreditFinished>();

        //            rider.AddConsumer<SimulateCreditCommandHandler>();

        //            rider.UsingKafka((context, k) =>
        //            {
        //                k.Host(configuration.GetkafkaConnectionString());
        //                k.TopicEndpoint<SimulateCreditCommand>(configuration.GetkafkaConsumer(), e =>
        //                {
        //                    e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
        //                    e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

        //                    e.ConfigureConsumer<SimulateCreditCommandHandler>(context);
        //                });
        //            });
        //        });
        //    }
        //    else
        //    {
        //        x.SetKebabCaseEndpointNameFormatter();
        //        x.AddConsumer<SimulateCreditCommandHandler>();

        //        x.UsingAzureServiceBus((context, cfg) =>
        //        {
        //            cfg.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
        //            cfg.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

        //            cfg.Host(configuration.GetAzServiceBusConnectionString());
                    
        //            cfg.ConfigureEndpoints(context);
        //        });
        //    }
        //});
    }

    public static void ConfigureWolverine(this WolverineOptions opts, IConfiguration configuration)
    {
        opts.UseKafka(configuration.GetkafkaConnectionString())
            .ConfigureConsumers(consumer => consumer = configuration.GetkafkaConsumer())
            .ConfigureProducers(producer => producer = configuration.GetkafkaProducer());

        //opts.PublishAllMessages().ToKafkaTopic("applications");

        opts.PublishMessage<SimulationCreditFinished>().ToKafkaTopic("simulations").TelemetryEnabled(true);

        opts.ListenToKafkaTopic("simulations")
            .ProcessInline().TelemetryEnabled(true);

        opts.Services.AddResourceSetupOnStartup();

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

