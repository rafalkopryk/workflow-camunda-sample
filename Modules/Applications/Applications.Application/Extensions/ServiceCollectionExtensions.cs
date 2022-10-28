namespace Applications.Application.Extensions;

using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.RegisterApplication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(RegisterApplicationCommand));
        services.AddDbContext<CreditApplicationDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("Default"), b => b.MigrationsAssembly("Applications.WebApi")));
    }

    public static void ConfigureApplication(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider.GetRequiredService<CreditApplicationDbContext>();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}

