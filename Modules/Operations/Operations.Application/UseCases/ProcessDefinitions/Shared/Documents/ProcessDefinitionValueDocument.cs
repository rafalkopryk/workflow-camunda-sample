using System.Text.Json.Serialization;

namespace Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;

public record ProcessDefinitionValueDocument
{
    [JsonPropertyName("bpmnProcessId")]
    public string BpmnProcessId { get; init; }

    [JsonPropertyName("version")]
    public int? Version { get; init; }

    [JsonPropertyName("resourceName")]
    public string? ResourceName { get; init; }

    [JsonPropertyName("resource")]
    public string? Resource { get; init; } // TODO remove

    [JsonPropertyName("processDefinitionKey")]
    public long? ProcessDefinitionKey { get; init; }

}
