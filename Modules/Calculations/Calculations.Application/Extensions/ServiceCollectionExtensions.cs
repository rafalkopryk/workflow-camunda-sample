namespace Calculations.Application.Extensions;

using Calculations.Application.Infrastructure.Database;
using Calculations.Application.UseCases.SimulateCreditCommand;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Application.Extensions;
using Processes.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration.IsCosmosDb()
            ? services.AddDbContextPool<CreditCalculationDbContext>(options => options.UseCosmos(configuration.GetAzCosmosDBConnectionString(), "Credit_Calculations"))
            : services.AddDbContextPool<CreditCalculationDbContext>(options => options.UseSqlServer(configuration.GetSqlConnectionString(), b => b.MigrationsAssembly("Calculations.WebApi")));

        services.AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            if (configuration.IsKafka())
            {
                x.AddRider(rider =>
                {
                    rider.AddProducer<string, SimulationCreditFinished>();

                    rider.AddConsumer<SimulateCreditCommandHandler>();

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host(configuration.GetkafkaConnectionString());
                        k.TopicEndpoint<SimulateCreditCommand>(configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<SimulateCreditCommandHandler>(context);
                        });
                    });
                });
            }
            else
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<SimulateCreditCommandHandler>();

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                    cfg.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                    cfg.Host(configuration.GetAzServiceBusConnectionString());
                    
                    cfg.ConfigureEndpoints(context);
                });
            }
        });
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

