using System.Text.Json;

namespace Processes.Operaton.WebApi.Operaton;

public sealed record OperatonExternalTask(
    string Id,
    string TopicName,
    string ProcessInstanceId,
    string? BusinessKey,
    int? Retries,
    Dictionary<string, object> Variables)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public T GetVariables<T>() => JsonSerializer.Deserialize<T>(
        JsonSerializer.Serialize(Variables),
        SerializerOptions) ?? throw new InvalidOperationException($"Could not deserialize variables for task {Id}.");
}
