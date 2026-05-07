# Camunda 8 - Specifics

## XML Namespaces

Required for Camunda 8 (Zeebe):

```xml
<bpmn:definitions
    xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
    xmlns:zeebe="http://camunda.org/schema/zeebe/1.0"
    xmlns:modeler="http://camunda.org/schema/modeler/1.0"
    modeler:executionPlatform="Camunda Cloud"
    modeler:executionPlatformVersion="8.9.0">
```

## Service Task Definition

```xml
<bpmn:serviceTask id="Activity_DoSomething">
  <bpmn:extensionElements>
    <zeebe:taskDefinition type="do-something:1" retries="3" />
  </bpmn:extensionElements>
</bpmn:serviceTask>
```

## User Task

```xml
<bpmn:userTask id="Activity_ApproveRequest">
  <bpmn:extensionElements>
    <zeebe:userTask />
    <zeebe:assignmentDefinition assignee="=assigneeUserId" />
  </bpmn:extensionElements>
</bpmn:userTask>
```

## Input/Output Mapping

```xml
<bpmn:serviceTask id="Activity_SendEmail">
  <bpmn:extensionElements>
    <zeebe:taskDefinition type="send-email:1" />
    <zeebe:ioMapping>
      <zeebe:input source="=customer.email" target="recipientEmail" />
      <zeebe:output source="=emailSent" target="notificationSent" />
    </zeebe:ioMapping>
  </bpmn:extensionElements>
</bpmn:serviceTask>
```

## Timer Events

```xml
<!-- Duration -->
<bpmn:timerEventDefinition>
  <bpmn:timeDuration>PT1H</bpmn:timeDuration>  <!-- ISO 8601 -->
</bpmn:timerEventDefinition>

<!-- Cycle -->
<bpmn:timerEventDefinition>
  <bpmn:timeCycle>R3/PT10M</bpmn:timeCycle>  <!-- 3 times every 10 min -->
</bpmn:timerEventDefinition>

<!-- Date -->
<bpmn:timerEventDefinition>
  <bpmn:timeDate>=dueDate</bpmn:timeDate>  <!-- FEEL expression -->
</bpmn:timerEventDefinition>
```

## FEEL Expressions

Camunda 8 uses FEEL (Friendly Enough Expression Language):

```xml
<!-- Condition -->
<bpmn:conditionExpression>=amount > 1000</bpmn:conditionExpression>

<!-- Variable mapping -->
<zeebe:input source="=customer.name" target="customerName" />

<!-- Message correlation -->
<zeebe:subscription correlationKey="=orderId" />
```

## Limits and Best Practices

- **Payload size**: max 4MB per process instance
- **Variable names**: no spaces, use camelCase
- **Retries**: default 3, configure in `zeebe:taskDefinition`
- **Timeouts**: configure in JobWorker, not in BPMN