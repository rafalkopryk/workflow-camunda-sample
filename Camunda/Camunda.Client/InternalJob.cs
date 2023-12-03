namespace Camunda.Client;

internal record InternalJob : IJob
{
    public long Key { get; init; }
    public string Type { get; init; }
    public long ProcessInstanceKey { get; init; }
    public string BpmnProcessId { get; init; }
    public int ProcessDefinitionVersion { get; init; }
    public long ProcessDefinitionKey { get; init; }
    public string ElementId { get; init; }
    public long ElementInstanceKey { get; init; }
    public string Worker { get; init; }
    public int Retries { get; init; }
    public DateTimeOffset Deadline { get; init; }
    public string Variables { get; init; }
    public string CustomHeaders { get; init; }
}
