using CamundaStartup.Aspire.Hosting.Camunda;
using CamundaStartup.Aspire.Hosting.Camunda.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var elastic = builder.AddElasticsearch("elastic")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithDataVolume("elastic")
    .WithLifetime(ContainerLifetime.Persistent);

var elasticConnectionString = elastic.Resource.GetConnectionStringExpressionWithoutCredentials();

var camunda = builder.AddCamunda("camunda", 8080)
    .WithElasticDatabase(elasticConnectionString)
    .WithOperate()
    .WithDataVolume("camunda2")
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(elastic);

builder.Build().Run();