# Camunda.Startup
A collection of .NET projects for integrating and hosting Camunda 8 workflow automation.

## Projects

### CamundaStartup.Aspire.Hosting.Camunda
.NET Aspire hosting extensions that provide `AddCamunda()` and resource definitions for configuring Camunda 8 inside a .NET Aspire AppHost. Supports multiple secondary storage backends (PostgreSQL, SQL Server, H2, Elasticsearch) and S3 backup.

### CamundaClient.Extensions
Lightweight job worker infrastructure for .NET applications using `Camunda.Orchestration.Sdk`. Provides `IJobHandler` / `IJobHandler<T>` interfaces, DI-scoped handler resolution, OpenTelemetry tracing via a dedicated `ActivitySource`, and `AddCamundaWorkers()` / `CreateJobWorker<T>()` extension methods.

## Demo

### CamundaStartup.Aspire.Hosting.Camunda.AppHost
Sample .NET Aspire AppHost that wires up all containers and services. Start with:
```bash
dotnet run --project Demo/CamundaStartup.Aspire.Hosting.Camunda.AppHost
```

### Camunda.Startup.DemoApp
Sample web API demonstrating a weather forecast workflow. Publishes a Camunda message on `POST /weatherforecast/{date}` and returns the cached result on `GET /weatherforecast/{date}`.

### CamundaStartup.ServiceDefaults
Shared configuration for OpenTelemetry (traces, metrics, logs), health checks, and service discovery. Referenced by all service projects.

## Requirements
- .NET 10
- Docker Desktop (for Camunda containers)
