namespace Common.Zeebe;

public interface IJob
{
    long Key { get; }
    string Type { get; }
    long ProcessInstanceKey { get; }
    string BpmnProcessId { get; }
    int ProcessDefinitionVersion { get; }
    long ProcessDefinitionKey { get; }
    string ElementId { get; }
    long ElementInstanceKey { get; }
    string Worker { get; }
    int Retries { get; }
    DateTime Deadline { get; }
    string Variables { get; }
    string CustomHeaders { get; }
}
