using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Application.Cqrs;

public interface IRequestHandler<TInput, TOutput>
{
    Task<TOutput> Handle(TInput input, CancellationToken cancellationToken = default);
}

public class Mediator(IServiceScopeFactory serviceScopeFactory)
{
    public async virtual Task<TOutput> Send<TInput, TOutput>(TInput input, CancellationToken cancellationToken = default)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TInput, TOutput>>();

        return await handler.Handle(input, cancellationToken);
    }
}

public static class CqrsExtensions
{
    public static void RegisterHandlersFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton<Mediator>();

        foreach (var assembly in assemblies)
        {
            var commandHandlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                .ToArray();

            foreach (var handlerType in commandHandlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType &&
                               i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}
