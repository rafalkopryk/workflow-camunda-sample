# CamundaStartup.Aspire.Hosting.Camunda library

Provides extension methods and resource definitions for a .NET Aspire AppHost to configure Camunda resources.

## Getting started

### Install the package

In your AppHost project, install the .NET Aspire Camunda Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
TODO
```

## Usage example

Then, in the _Program.cs_ file of `AppHost`, add Camunda resources using the following methods:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var camunda = builder.AddCamunda("camunda", 8080);

builder.AddProject<Projects.MyApp>("myapp")
       .WithReference(camunda)
       .WaitFor(camunda);

builder.Build().Run();
```

To use the elastic persistence and Operate, you can also add the following methods:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var elastic = builder.AddElasticsearch("elastic")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithDataVolume("elastic")
    .WithLifetime(ContainerLifetime.Persistent);

var elasticConnectionString = ReferenceExpression.Create(
            $"http://{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}"); ;

var camunda = builder.AddCamunda("camunda", 8080)
    .WithElasticDatabase(elasticConnectionString)
    .WithOperate()
    .WithDataVolume("camunda")
    .WithLifetime(ContainerLifetime.Persistent)
    .WaitFor(elastic);

builder.AddProject<Projects.MyApp>("myapp")
       .WithReference(camunda)
       .WaitFor(camunda);

builder.Build().Run();
```
Integration with Elasticsearch in the example is achieved using the NuGet package `Aspire.Hosting.Elasticsearch`.
