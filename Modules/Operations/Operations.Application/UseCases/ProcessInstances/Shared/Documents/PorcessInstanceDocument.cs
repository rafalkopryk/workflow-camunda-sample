using System.Text.Json.Serialization;

public record PorcessInstanceDocument
{
    [JsonPropertyName("timestamp")]

    public long Timestamp { get; init; }

    [JsonPropertyName("value")]
    public PorcessInstanceValueDocument Value { get; init; }

    [JsonPropertyName("intent")]
    public string Intent { get; init; }
}