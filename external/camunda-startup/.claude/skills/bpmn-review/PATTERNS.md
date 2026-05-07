# BPMN Patterns - Best Practices

## Service Task - Type

Format: `task-name:version`

```xml
<!-- Good -->
<zeebe:taskDefinition type="weather-forecast-retrieve:1" />
<zeebe:taskDefinition type="send-email:1" />
<zeebe:taskDefinition type="validate-order:2" />

<!-- Bad -->
<zeebe:taskDefinition type="weatherForecast" />  <!-- missing version -->
<zeebe:taskDefinition type="task1" />            <!-- non-descriptive name -->
```

## Element ID Naming

Use descriptive IDs with element type prefix:

```xml
<!-- Good -->
<bpmn:startEvent id="StartEvent_OrderReceived" />
<bpmn:serviceTask id="Activity_ValidateOrder" />
<bpmn:endEvent id="Event_OrderCompleted" />

<!-- Bad -->
<bpmn:startEvent id="Event_0abc123" />  <!-- auto-generated -->
<bpmn:serviceTask id="Activity_1" />     <!-- non-descriptive -->
```

## Message Events

Every message event should have:
1. Descriptive message name
2. Correlation key (for intermediate events)

```xml
<!-- Start event - correlation key optional -->
<bpmn:message id="Message_OrderReceived" name="Message_OrderReceived" />

<!-- Intermediate event - correlation key required -->
<bpmn:message id="Message_PaymentConfirmed" name="Message_PaymentConfirmed">
  <bpmn:extensionElements>
    <zeebe:subscription correlationKey="=orderId" />
  </bpmn:extensionElements>
</bpmn:message>
```

## .NET Handler - Pattern

For each service task, create a handler:

```csharp
[JobWorker(Type = "task-name:1")]
public class TaskNameJobHandler : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken ct)
    {
        var input = job.GetVariablesAsType<TaskNameInput>();

        // Logic...

        // Auto-complete is enabled by default
    }
}

public record TaskNameInput(string RequiredField);
```

## Error Handling

For critical tasks, add error boundary event:

```xml
<bpmn:serviceTask id="Activity_ProcessPayment">
  <bpmn:extensionElements>
    <zeebe:taskDefinition type="process-payment:1" />
  </bpmn:extensionElements>
</bpmn:serviceTask>
<bpmn:boundaryEvent id="BoundaryEvent_PaymentFailed" attachedToRef="Activity_ProcessPayment">
  <bpmn:errorEventDefinition id="ErrorEventDefinition_Payment" errorRef="Error_PaymentFailed" />
</bpmn:boundaryEvent>
```

## Worker Registration

In `Program.cs`:

```csharp
builder.Services.AddCamunda(
    options => options.Endpoint = connectionString,
    builder => builder
        .AddWorker<WeatherForecastRetrieveJobHandler>()
        .AddWorker<SendEmailJobHandler>());
```