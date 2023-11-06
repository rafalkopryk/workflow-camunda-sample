using Operations.Application.UseCases.Incidents.Shared.Documents;
using System.Text.Json.Serialization;

namespace Operations.Application.Incidents.SearchProcessIncidents.Shared.Documents;

public record ProcessIncidentDocument
{
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; init; }

    [JsonPropertyName("value")]
    public ProcessIncidentValueDocument Value { get; init; }

    [JsonPropertyName("intent")]
    public string Intent { get; init; }

    [JsonPropertyName("valueType")]
    public string ValueType { get; init; }

    [JsonPropertyName("key")]
    public long? Key { get; init; }
}