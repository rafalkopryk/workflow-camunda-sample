namespace Applications.Application.Extensions;

using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.RegisterApplication;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(RegisterApplicationCommand));

        services.AddDbContext<CreditApplicationDbContext>(optionsAction => optionsAction.UseInMemoryDatabase("Credit.Applications"));
    }
}

