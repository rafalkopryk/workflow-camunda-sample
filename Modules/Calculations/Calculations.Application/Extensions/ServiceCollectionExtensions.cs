namespace Calculations.Application.Extensions;

using Calculations.Application.Infrastructure.Database;
using Calculations.Application.UseCases.SimulateCreditCommand;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(SimulateCreditCommand));
        services.AddDbContext<CreditCalculationDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("Default"), b => b.MigrationsAssembly("Calculations.WebApi")));
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

