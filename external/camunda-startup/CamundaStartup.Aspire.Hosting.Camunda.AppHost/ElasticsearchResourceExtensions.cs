namespace CamundaStartup.Aspire.Hosting.Camunda.AppHost
{
    internal static class ElasticsearchResourceExtensions
    {
        public static ReferenceExpression GetConnectionStringExpressionWithoutCredentials(this ElasticsearchResource resource)
        {
            ArgumentNullException.ThrowIfNull(resource);
            return ReferenceExpression.Create(
                $"http://{resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{resource.PrimaryEndpoint.Property(EndpointProperty.Port)}");
        }
    }
}
