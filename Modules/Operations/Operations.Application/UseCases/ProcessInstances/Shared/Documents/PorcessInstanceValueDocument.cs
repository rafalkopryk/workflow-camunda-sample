using System.Text.Json.Serialization;

public record PorcessInstanceValueDocument
{
    [JsonPropertyName("bpmnProcessId")]
    public string BpmnProcessId { get; init; }

    [JsonPropertyName("processInstanceKey")]
    public long ProcessInstanceKey { get; init; }

    [JsonPropertyName("elementId")]
    public string ElementId { get; init; }

    [JsonPropertyName("bpmnElementType")]
    public string BpmnElementType { get; init; }

    [JsonPropertyName("version")]
    public int? Version { get; init; }

    [JsonPropertyName("processDefinitionKey")]
    public long? ProcessDefinitionKey { get; init; }
}
