using System.Text.Json.Serialization;

public record FlowNodeInstanceDocument
{
    [JsonPropertyName("timestamp")]

    public long Timestamp { get; init; }

    [JsonPropertyName("value")]
    public FlowNodeInstanceValueDocument Value { get; init; }

    [JsonPropertyName("intent")]
    public string Intent { get; init; }

    [JsonPropertyName("valueType")]
    public string ValueType { get; init; }

    [JsonPropertyName("key")]
    public long Key { get; init; }
}