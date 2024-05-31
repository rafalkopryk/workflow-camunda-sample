namespace Applications.Application.Extensions;

using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.CloseApplication;
using Applications.Application.UseCases.SetDecision;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Common.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration.IsCosmosDb()
            ? services.AddDbContextPool<CreditApplicationDbContext>(options => options.UseCosmos(configuration.GetAzCosmosDBConnectionString(), "Credit_Applications"))
            : services.AddDbContextPool<CreditApplicationDbContext>(options => options.UseSqlServer(configuration.GetSqlConnectionString(), b => b.MigrationsAssembly("Applications.WebApi")));

        services.AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            //x.AddConsumer<SetDecisionCommandCommandHandler>();
            x.AddConsumer<CloseApplicationCommandHandler>();

            if (configuration.IsKafka())
            {
                x.AddRider(configure =>
                {
                    configure.UsingKafka((context, cfg) =>
                    {
                        cfg.Host(configuration.GetkafkaConnectionString());
                        cfg.TopicEndpoint<CloseApplicationCommand>("command.credit.applications.close.v1", configuration.GetkafkaConsumer(), e =>
                        {
                            e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                            e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                            e.ConfigureConsumer<CloseApplicationCommandHandler>(context);
                        });

                        //cfg.TopicEndpoint<SetDecisionCommand>("command.credit.applications.decision.v1", configuration.GetkafkaConsumer(), e =>
                        //{
                        //    e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                        //    e.UseRawJsonDeserializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);

                        //    e.ConfigureConsumer<SetDecisionCommandCommandHandler>(context);
                        //});
                    });
                });
            }
            else
            {
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

        var context = serviceProvider.GetRequiredService<CreditApplicationDbContext>();
        //if (context.Database.GetPendingMigrations().Any())
        //{
        //    context.Database.Migrate();
        //}
        await context.Database.EnsureCreatedAsync();
    }
}

