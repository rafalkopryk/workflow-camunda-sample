namespace Operations.Application.Extensions;

using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(ServiceCollectionExtensions).Assembly));
        services.AddSingleton(new ElasticsearchClient(new Uri(configuration.GetValue<string>("Elasticsearch:Endpoint"))));
    }
}
