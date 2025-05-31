using CamundaStartup.Aspire.Hosting.Camunda;

namespace Aspire.AppHost;

public static class CamundaBuilderExtensions
{
    public static IResourceBuilder<T> WithCamundaReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<CamundaResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithEnvironment("Camunda__CamundaRest__Endpoint", source.Resource.ConnectionStringExpression)
            .WithEnvironment("Camunda__CamundaGrpc__Endpoint", source.Resource.GrpcConnectionStringExpression)
            .WithEnvironment("Camunda__JobWorkers__Default__TimeoutInMs", "30000")
            .WithEnvironment("Camunda__JobWorkers__Default__PollingMaxJobsToActivate", "5")
            .WithEnvironment("Camunda__JobWorkers__Default__PollingRequestTimeoutInMs", "-1")
            .WithEnvironment("Camunda__JobWorkers__Default__PollingDelayInMs", "10000")
            .WithEnvironment("Camunda__JobWorkers__Default__UseStream", "true")
            .WithEnvironment("Camunda__JobWorkers__Default__StreamTimeoutInSec", "900");
    }    
}