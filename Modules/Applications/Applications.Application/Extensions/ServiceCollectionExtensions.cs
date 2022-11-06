namespace Applications.Application.Extensions;

using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.CloseApplication;
using Applications.Application.UseCases.SetDecision;
using Common.Zeebe;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(typeof(SetDecisionCommand));
        services.AddDbContextPool<CreditApplicationDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("Default"), b => b.MigrationsAssembly("Applications.WebApi")));

        services.AddZeebe(
            options => configuration.GetSection("ZEEBE").Bind(options),
            builder => builder
                .AddService()
                .AddTaskWorker<CloseApplicationCommand>()
                .AddTaskWorker<SetDecisionCommand>());
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

