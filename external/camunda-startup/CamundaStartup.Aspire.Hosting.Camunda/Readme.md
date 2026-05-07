# CamundaStartup.Aspire.Hosting.Camunda library

Provides extension methods and resource definitions for a .NET Aspire AppHost to configure Camunda resources.

## Getting started

### Install the package

In your AppHost project, install the .NET Aspire Camunda Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
TODO
```

## Basic usage (H2 — default)

H2 is an embedded file-based database bundled with Camunda. No extra NuGet package is required. Data is persisted inside the Camunda container volume at `/usr/local/camunda/data`.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var h2Password = builder.AddParameter("h2Password", "", secret: true);
var h2JdbcUrl = ReferenceExpression.Create(
    $"jdbc:h2:file:/usr/local/camunda/data/h2/camunda;DB_CLOSE_DELAY=-1;AUTO_SERVER=TRUE");

var camunda = builder.AddCamunda("camunda", 8080)
    .WithDataVolume("camunda")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRdmbsDatabase(h2JdbcUrl, ReferenceExpression.Create($"sa"), h2Password.Resource);

builder.AddProject<Projects.MyApp>("myapp")
       .WithReference(camunda)
       .WaitFor(camunda);

builder.Build().Run();
```

## Alternative storage backends

### Elasticsearch

Requires the `Aspire.Hosting.Elasticsearch` NuGet package.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var elastic = builder.AddElasticsearch("elasticsearch")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithDataVolume("elastic")
    .WithLifetime(ContainerLifetime.Persistent);

var elasticUrl = ReferenceExpression.Create(
    $"http://{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}");

var camunda = builder.AddCamunda("camunda", 8080)
    .WithDataVolume("camunda")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithElasticDatabase(elasticUrl)
    .WaitFor(elastic);

builder.AddProject<Projects.MyApp>("myapp")
       .WithReference(camunda)
       .WaitFor(camunda);

builder.Build().Run();
```

### PostgreSQL

Requires the `Aspire.Hosting.PostgreSQL` NuGet package.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("camunda-database", "camunda");

var camunda = builder.AddCamunda("camunda", 8080)
    .WithDataVolume("camunda")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRdmbsDatabase(
        db.Resource.JdbcConnectionString,
        postgres.Resource.UserNameReference,
        postgres.Resource.PasswordParameter)
    .WaitFor(postgres);

builder.AddProject<Projects.MyApp>("myapp")
       .WithReference(camunda)
       .WaitFor(camunda);

builder.Build().Run();
```

### SQL Server

Requires the `Aspire.Hosting.SqlServer` NuGet package.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sqlServer.AddDatabase("camunda-database", "camunda");

var camunda = builder.AddCamunda("camunda", 8080)
    .WithDataVolume("camunda")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRdmbsDatabase(
        db.Resource.JdbcConnectionString,
        sqlServer.Resource.UserNameReference,
        sqlServer.Resource.PasswordParameter)
    .WaitFor(sqlServer);

builder.AddProject<Projects.MyApp>("myapp")
       .WithReference(camunda)
       .WaitFor(camunda);

builder.Build().Run();
```

## S3 backup

You can configure S3-compatible backup storage using `WithS3Backup`:

```csharp
var accessKey = builder.AddParameter("s3AccessKey", secret: true);
var secretKey = builder.AddParameter("s3SecretKey", secret: true);

var camunda = builder.AddCamunda("camunda", 8080)
    .WithDataVolume("camunda")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithS3Backup(
        endpoint: ReferenceExpression.Create($"http://minio:9000"),
        accessKey: accessKey.Resource,
        secretKey: secretKey.Resource,
        bucketName: "camunda-backup");
```

## Connection strings

`WithReference(camunda)` injects both REST and gRPC connection strings into the referencing project:

| Connection string name | Format |
|------------------------|--------|
| `camunda` | `http://{host}:{port}/v2/` (REST) |
| `camunda__grpc` | `http://{host}:{port}` (gRPC) |
