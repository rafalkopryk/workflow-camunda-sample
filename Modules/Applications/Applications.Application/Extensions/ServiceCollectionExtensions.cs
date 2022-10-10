namespace Applications.Application.Extensions;

using Applications.Application.UseCases.RegisterApplication;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(typeof(RegisterApplicationCommand));
    }
}

