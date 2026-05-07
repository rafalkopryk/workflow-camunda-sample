# JobHandler Code Templates

## Basic JobHandler Template

```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;

namespace {Namespace};

public class {ClassName}JobHandler : IJobHandler
{
    public Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<{ClassName}Input>();

        // TODO: Implement business logic

        return Task.CompletedTask;
    }
}

public record {ClassName}Input(/* Add properties matching process variables */);
```

## JobHandler with Dependency Injection

```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;
using Microsoft.Extensions.Logging;

namespace {Namespace};

public class {ClassName}JobHandler(
    ILogger<{ClassName}JobHandler> logger,
    {IService} service) : IJobHandler
{
    public async Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        logger.LogInformation("Processing {JobType} job {JobKey}", job.Type, job.JobKey);

        var input = job.GetVariables<{ClassName}Input>();

        await service.ProcessAsync(input, ct);
    }
}

public record {ClassName}Input(/* Add properties */);
```

## JobHandler with Result

Use `IJobHandler<T>` when the worker must return output variables:

```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;

namespace {Namespace};

public record {ClassName}Output(/* Add properties */);

public class {ClassName}JobHandler : IJobHandler<{ClassName}Output>
{
    public Task<{ClassName}Output> HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<{ClassName}Input>();

        // Business logic...
        var output = new {ClassName}Output(/* ... */);

        return Task.FromResult(output);
    }
}

public record {ClassName}Input(/* Add properties */);
```

## Naming Conventions

| BPMN Task Type | Class Name | File Name |
|----------------|------------|-----------|
| `send-email:1` | `SendEmailJobHandler` | `SendEmailJobHandler.cs` |
| `validate-order:1` | `ValidateOrderJobHandler` | `ValidateOrderJobHandler.cs` |
| `process-payment:2` | `ProcessPaymentJobHandler` | `ProcessPaymentJobHandler.cs` |

### Converting Task Type to Class Name

1. Remove version suffix (`:1`, `:2`, etc.)
2. Convert kebab-case to PascalCase
3. Append `JobHandler` suffix

Examples:
- `send-notification:1` → `SendNotificationJobHandler`
- `calculate-shipping-cost:1` → `CalculateShippingCostJobHandler`
- `validate-customer-data:2` → `ValidateCustomerDataJobHandler`

## Worker Registration

### Fire-and-forget (IJobHandler)
```csharp
app.CreateJobWorker<{ClassName}JobHandler>(new JobWorkerConfig
{
    JobType = "{task-type}",
    JobTimeoutMs = 30_000,
});
```

### With Result (IJobHandler<T>)
```csharp
app.CreateJobWorker<{ClassName}JobHandler, {ClassName}Output>(new JobWorkerConfig
{
    JobType = "{task-type}",
    JobTimeoutMs = 30_000,
});
```

### Multiple Workers (chained)
```csharp
app.CreateJobWorker<FirstJobHandler>(new JobWorkerConfig { JobType = "first-task:1", JobTimeoutMs = 30_000 })
   .CreateJobWorker<SecondJobHandler, SecondOutput>(new JobWorkerConfig { JobType = "second-task:1", JobTimeoutMs = 30_000 })
   .CreateJobWorker<ThirdJobHandler>(new JobWorkerConfig { JobType = "third-task:1", JobTimeoutMs = 30_000 });
```

### With Poll Timeout
```csharp
app.CreateJobWorker<{ClassName}JobHandler>(new JobWorkerConfig
{
    JobType = "{task-type}",
    JobTimeoutMs = 30_000,
    PollTimeoutMs = 10_000,
});
```