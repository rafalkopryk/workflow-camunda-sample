using System.Text.Json.Serialization;

namespace Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;

public record ProcessDefinitionDocument
{
    [JsonPropertyName("timestamp")]

    public long Timestamp { get; init; }

    [JsonPropertyName("value")]
    public ProcessDefinitionValueDocument Value { get; init; }

    [JsonPropertyName("intent")]
    public string Intent { get; init; }

    [JsonPropertyName("valueType")]
    public string ValueType { get; init; }
}