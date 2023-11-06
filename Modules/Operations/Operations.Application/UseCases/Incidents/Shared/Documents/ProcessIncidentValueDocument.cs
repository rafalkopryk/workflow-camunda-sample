using System.Text.Json.Serialization;

namespace Operations.Application.UseCases.Incidents.Shared.Documents;

public record ProcessIncidentValueDocument
{
    [JsonPropertyName("elementId")]
    public string ElementId { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("processInstanceKey")]
    public long? ProcessInstanceKey { get; init; }

    [JsonPropertyName("processDefinitionKey")]
    public long? ProcessDefinitionKey { get; init; }
}
