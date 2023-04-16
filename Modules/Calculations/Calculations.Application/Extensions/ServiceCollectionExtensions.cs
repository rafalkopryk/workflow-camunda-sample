namespace Calculations.Application.Extensions;

using Calculations.Application.Infrastructure.Database;
using Calculations.Application.UseCases.SimulateCreditCommand;
using Common.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<CreditCalculationDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("Default"), b => b.MigrationsAssembly("Calculations.WebApi")));

        services.AddMediatR(typeof(SimulateCreditCommand));

        services.AddKafka(
            options => configuration.GetSection("EventBus").Bind(options),
            options => configuration.GetSection("EventBus").Bind(options),
            builder => builder
                .UseTopic<SimulateCreditCommand>());
    }

    public static void ConfigureApplication(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider.GetRequiredService<CreditCalculationDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}

