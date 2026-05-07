# CamundaClient.Extensions

Lightweight extensions for integrating Camunda 8 job workers into .NET applications using `Camunda.Orchestration.Sdk`.

## Usage

### 1. Define a Job Handler

Implement `IJobHandler` for fire-and-forget tasks:

```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;

public class MyJobHandler : IJobHandler
{
    public Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<MyInput>();
        // Process job...
        return Task.CompletedTask;
    }
}
```

Implement `IJobHandler<T>` when the worker should return variables:

```csharp
public record MyOutput(string Status);

public class MyJobHandlerWithResult : IJobHandler<MyOutput>
{
    public Task<MyOutput> HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        return Task.FromResult(new MyOutput("done"));
    }
}
```

### 2. Register Workers

```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;

// Register the hosted service that runs all workers
builder.AddCamundaWorkers();

var app = builder.Build();

// Fire-and-forget handler
app.CreateJobWorker<MyJobHandler>(new JobWorkerConfig
{
    JobType = "my-task:1",
    JobTimeoutMs = 30_000,
});

// Handler with result
app.CreateJobWorker<MyJobHandlerWithResult, MyOutput>(new JobWorkerConfig
{
    JobType = "my-result-task:1",
    JobTimeoutMs = 30_000,
});

// Raw async delegate
app.CreateJobWorker(new JobWorkerConfig
{
    JobType = "my-other-task:1",
    JobTimeoutMs = 30_000,
}, async (job, ct) =>
{
    // Handle job inline
});
```

Calls can be chained since all overloads return `IHost`.

### 3. OpenTelemetry

Each job handler execution is automatically wrapped in an OpenTelemetry `Activity` (source name: `"Camunda.Client.Extensions"`). Register the source in your service defaults:

```csharp
tracing.AddSource("Camunda.Client.Extensions");
```

Each span carries the following tags:

| Tag | Value |
|-----|-------|
| `messaging.system` | `camunda` |
| `messaging.destination` | job type |
| `messaging.operation` | `process` |
| `camunda.job_key` | job key |
| `camunda.element_id` | BPMN element ID |
| `camunda.process_definition_id` | process definition ID |
| `camunda.process_instance_key` | process instance key |

Exceptions are recorded on the span and the status is set to `Error` before rethrowing.

## Key Features

- **Type-safe results** — `IJobHandler<T>` returns `Task<T>` with compile-time type safety
- **DI-friendly** — handlers are resolved per job via `ActivatorUtilities.CreateInstance`, so constructor injection works
- **Scoped lifetime** — each job execution gets its own `IServiceScope`
- **Built-in tracing** — every handler invocation produces an OTel span with Camunda-specific tags
- **Minimal setup** — `AddCamundaWorkers()` + `CreateJobWorker<T>()` is all you need
