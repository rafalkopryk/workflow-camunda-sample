using CamundaStartup.Aspire.Hosting.Camunda;
using CamundaStartup.Aspire.Hosting.Camunda.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var elastic = builder.AddElasticsearch("elasticsearch")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithDataVolume("elastic")
    .WithLifetime(ContainerLifetime.Persistent);

var elasticConnectionString = elastic.Resource.GetConnectionStringExpressionWithoutCredentials();
var camunda = builder.AddCamunda("camunda", 8080, elasticConnectionString)
    .WithDataVolume("camunda2")
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(elastic);

var demoApp = builder.AddProject<Projects.Camunda_Startup_DemoApp>("DemoApp")
    .WithReference(camunda, "camunda")
    .WaitFor(camunda);

builder.Build().Run();