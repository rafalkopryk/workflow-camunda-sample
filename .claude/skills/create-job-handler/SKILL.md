---
description: Generates JobHandler classes for service tasks in BPMN files
---

# Create JobHandler Skill

Activate when user:
- Asks to create handlers for a BPMN file
- Wants to generate JobHandler classes from a workflow
- Requests scaffolding for service tasks
- Mentions "create handler" or "generate handler" with BPMN context

## Workflow

1. **Parse BPMN File**
   - Read the specified `.bpmn` file
   - Extract all `<bpmn:serviceTask>` elements
   - Find task types from `<zeebe:taskDefinition type="..." />` extensions

2. **Identify Missing Handlers**
   - Search codebase for existing classes implementing `IJobHandler` or `IJobHandler<T>`
   - Cross-reference with `CreateJobWorker<T>()` registrations in `Program.cs`
   - List service tasks that need handlers

3. **Ask User for Location**
   - Determine target project/directory for new handlers
   - Default: same directory as existing handlers or `Feature/` folder

4. **Generate JobHandler Classes**
   For each missing service task, generate:
   - Class file named `{PascalCaseName}JobHandler.cs`
   - Implements `IJobHandler` (or `IJobHandler<T>` if outputs are needed)
   - Input record for process variables (if extractable from BPMN)

5. **Update Worker Registration**
   - Show user how to register new workers in `Program.cs`
   - Provide `app.CreateJobWorker<T>()` code snippet

## BPMN Parsing Reference

### Service Task Structure
```xml
<bpmn:serviceTask id="Activity_DoSomething" name="Do Something">
  <bpmn:extensionElements>
    <zeebe:taskDefinition type="do-something:1" />
  </bpmn:extensionElements>
</bpmn:serviceTask>
```

### Extracting Task Type
- Look for `zeebe:taskDefinition` element inside `bpmn:extensionElements`
- The `type` attribute contains the job type (e.g., `do-something:1`)
- Naming convention: `task-name:version`

### Input/Output Mappings
```xml
<zeebe:ioMapping>
  <zeebe:input source="=variable" target="localVar" />
  <zeebe:output source="=result" target="outputVar" />
</zeebe:ioMapping>
```

## Code Generation Templates

See [TEMPLATES.md](./TEMPLATES.md) for JobHandler code templates.

## Example Output

For a service task with type `send-notification:1`:

```csharp
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk.Runtime;

namespace MyApp.Feature;

public class SendNotificationJobHandler : IJobHandler
{
    public Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<SendNotificationInput>();

        // TODO: Implement notification logic

        return Task.CompletedTask;
    }
}

public record SendNotificationInput(/* Add expected properties */);
```

## Registration Snippet

```csharp
app.CreateJobWorker<SendNotificationJobHandler>(new JobWorkerConfig
{
    JobType = "send-notification:1",
    JobTimeoutMs = 30_000,
});
```

## Related Files

- `CamundaClient.Extensions/IJobHandler.cs` — `IJobHandler`, `IJobHandler<T>`
- `CamundaClient.Extensions/CamundaWorkerExtensions.cs` — `AddCamundaWorkers()`, `CreateJobWorker<T>()`, `CreateJobWorker<T, TOutput>()`
- `Demo/Camunda.Startup.DemoApp/Feature/` — Example handlers
- `Demo/Camunda.Startup.DemoApp/Program.cs` — Worker registration
