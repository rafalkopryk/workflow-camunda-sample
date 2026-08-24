using System.Text.Json;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Shared;

internal static class ConductorTaskInputExtensions
{
    extension(IReadOnlyDictionary<string, object> inputData)
    {
        public T GetVariables<T>()
        {
            var json = JsonSerializer.Serialize(inputData);

            return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web)
                   ?? throw new InvalidOperationException(
                       $"Conductor task input could not be deserialized to {typeof(T).Name}.");
        }
    }
}
