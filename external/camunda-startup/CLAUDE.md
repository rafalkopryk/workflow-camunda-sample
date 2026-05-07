# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## IMPORTANT

- All Markdown files and documentation must be written in English
- Prefer Aspire NuGet integrations when available

# Build & Run Commands

## Using Aspire CLI (Recommended)

```bash
# Start the application
aspire start

# Start in isolated mode (for worktrees/preventing port conflicts)
aspire start --isolated

# Stop the application
aspire stop
```

## Using dotnet CLI

```bash
# Build entire solution
dotnet build CamundaStartup.sln

# Run the Aspire AppHost (starts all services including Camunda containers)
dotnet run --project Demo/CamundaStartup.Aspire.Hosting.Camunda.AppHost
```

**Prerequisites:** Docker Desktop must be running for container services.

### Secondary Storage Configuration

The secondary storage backend is configured via Aspire parameters. Options: `postgres`, `sqlserver`, `h2`, `elastic` (default).

**Option 1:** Edit `Demo/CamundaStartup.Aspire.Hosting.Camunda.AppHost/appsettings.json`:
```json
{
  "Parameters": {
    "secondaryStorage": "postgres"
  }
}
```

**Option 2:** Use environment variable:
```bash
Parameters__secondaryStorage=postgres dotnet run --project Demo/CamundaStartup.Aspire.Hosting.Camunda.AppHost
```

## Project Structure

```
CamundaStartup/
├── CamundaClient.Extensions/          # Job worker infrastructure (IJobHandler, IJobHandler<T>, OTel)
├── CamundaStartup.Aspire.Hosting.Camunda/  # Aspire hosting extensions
└── Demo/
    ├── Camunda.Startup.DemoApp/       # Sample web API with weather forecast workflow
    ├── CamundaStartup.Aspire.Hosting.Camunda.AppHost/  # Aspire orchestration entry point
    └── CamundaStartup.ServiceDefaults/  # Shared OpenTelemetry & resilience config
```

## Architecture Overview

This is a .NET 10 / Aspire 13 solution for integrating Camunda 8 workflow automation into .NET applications.

### Projects

| Project                                            | Description                                                          |
|----------------------------------------------------|----------------------------------------------------------------------|
| **CamundaClient.Extensions**                       | Job worker infrastructure — `IJobHandler`, `IJobHandler<T>`, DI scoping, OTel tracing |
| **CamundaStartup.Aspire.Hosting.Camunda**          | Aspire extensions — `AddCamunda()`, storage backends, S3 backup      |
| **Camunda.Startup.DemoApp**                        | Sample web API demonstrating weather forecast workflow               |
| **CamundaStartup.Aspire.Hosting.Camunda.AppHost**  | Aspire orchestration host — wires up all containers and services     |
| **CamundaStartup.ServiceDefaults**                 | Shared configuration — OpenTelemetry, resilience, service discovery  |

### Demo Application Flow

```
POST /weatherforecast/{date}
  → Camunda (async) → Creates service task
  → Worker picks up task → JobHandler processes → Caches result
  → API returns 202 Accepted immediately

GET /weatherforecast/{date}
  → Returns cached result from memory
```

### Key Patterns

**Worker Hosted Service Registration:**
```csharp
// In Program.cs — registers the background service that drives all workers
builder.AddCamundaWorkers();

var app = builder.Build();

app.CreateJobWorker<MyJobHandler>(new JobWorkerConfig
{
    JobType = "my-task:1",
    JobTimeoutMs = 30_000,
});
```

**Job Handler (fire-and-forget):**
```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;

public class MyJobHandler : IJobHandler
{
    public Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<MyInput>();
        // Process job — auto-completes after return
        return Task.CompletedTask;
    }
}
```

**Job Handler with Result:**
```csharp
public record MyOutput(string Status);

public class MyJobHandler : IJobHandler<MyOutput>
{
    public Task<MyOutput> HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<MyInput>();
        return Task.FromResult(new MyOutput("done"));
    }
}

// Registration — specify both handler type and output type
app.CreateJobWorker<MyJobHandler, MyOutput>(new JobWorkerConfig
{
    JobType = "my-task:1",
    JobTimeoutMs = 30_000,
});
```

### JobWorkerConfig Options

| Property | Description |
|----------|-------------|
| `JobType` | Job type matching BPMN service task |
| `JobTimeoutMs` | Job lock timeout in ms |
| `PollTimeoutMs` | Polling request timeout in ms |

### BPMN Deployment Pattern

Deploy workflow definitions on startup using a background service:
```csharp
public class DeployBPMNDefinitionService(ICamundaClientRest client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var file = await File.ReadAllBytesAsync("my-workflow.bpmn", stoppingToken);
        using var stream = new MemoryStream(file, writable: false);
        await client.DeploymentsAsync([new FileParameter(stream, "my-workflow.bpmn")], string.Empty, stoppingToken);
    }
}
```

## Service Endpoints

When running via AppHost:

| Service            | Port  | Protocol |
|--------------------|-------|----------|
| DemoApp            | 7230  | HTTPS    |
| Camunda REST       | 8080  | HTTP     |
| Camunda gRPC       | 26500 | HTTP/2   |
| Camunda Management | 9600  | HTTP     |

## Testing with .http Files

Use JetBrains HTTP Client files in Rider:
- `Demo/Camunda.Startup.DemoApp/Camunda.Startup.DemoApp.http` - API testing
