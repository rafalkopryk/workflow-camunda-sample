using System.Text.Json;

namespace Camunda.Client.Jobs;

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
    DateTimeOffset Deadline { get; }
    string Variables { get; }
    string CustomHeaders { get; }

    public T GetVariablesAsType<T>()
    {
        return JsonSerializer.Deserialize<T>(Variables, JsonSerializerCustomOptions.CamelCase);
    }
}
