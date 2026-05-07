---
description: Automatically analyzes BPMN files for Camunda 8 best practices and project compliance
---

# BPMN Review Skill

Activate when user:
- Asks about a `.bpmn` file
- Edits or adds a BPMN workflow
- Requests a process review
- Mentions service tasks, message events, or workflows

## Analysis Steps

1. **Read the BPMN file** and identify:
   - Service tasks and their types (`zeebe:taskDefinition`)
   - Message events and correlation keys
   - User tasks
   - Error boundary events

2. **Check compliance with PATTERNS.md**:
   - Do service task types follow `name:version` format?
   - Are element IDs descriptive (not auto-generated)?
   - Do message events have correlation keys?

3. **Check compliance with .NET code**:
   - Do handlers exist for all service tasks?
   - Search for classes implementing `IJobHandler` or `IJobHandler<T>`
   - Cross-reference job types with `CreateJobWorker<T>()` calls in `Program.cs`
   - Compare BPMN task types with registered workers

4. **Verify against CAMUNDA_SPECIFICS.md**:
   - Are correct Zeebe namespaces used?
   - Is configuration compatible with Camunda 8?

## Reporting

Report:
- **Errors**: missing handlers, incorrect types
- **Warnings**: missing correlation keys, auto-generated IDs
- **Suggestions**: improvements, missing error handling

## Related Project Files

- Handlers: `Demo/Camunda.Startup.DemoApp/**/*Handler.cs`
- Messages: classes with `[CamundaMessage]` attribute
- Handler pattern: see PATTERNS.md