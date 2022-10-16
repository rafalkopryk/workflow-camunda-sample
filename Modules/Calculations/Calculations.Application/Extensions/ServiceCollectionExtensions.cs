namespace Calculations.Application.Extensions;

using Calculations.Application.Infrastructure.Database;
using Calculations.Application.UseCases.SimulateCreditCommand;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(SimulateCreditCommand));

        services.AddDbContext<CreditCalculationDbContext>(optionsAction => optionsAction.UseInMemoryDatabase("Credit.Calcualtions"));
    }
}

