namespace CamundaStartup.Aspire.Hosting.Camunda.AppHost;

internal static class ElasticsearchResourceExtensions
{
    extension(ElasticsearchResource resource)
    {
        public ReferenceExpression GetConnectionStringExpressionWithoutCredentials()
        {
            ArgumentNullException.ThrowIfNull(resource);
            return ReferenceExpression.Create(
                $"http://{resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{resource.PrimaryEndpoint.Property(EndpointProperty.Port)}");
        }
    }
}
