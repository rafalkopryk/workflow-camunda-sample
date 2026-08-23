namespace Processes.Application.Utils;

public record ProcessDefinition
(
    long Key,
    string Name,
    long Version,
    string BpmnProcessId
 );
