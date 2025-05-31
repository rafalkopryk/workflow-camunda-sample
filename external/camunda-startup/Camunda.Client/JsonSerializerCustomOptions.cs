using System.Text.Json;

namespace Camunda.Client;

public static class JsonSerializerCustomOptions
{
    public static readonly JsonSerializerOptions CamelCase = new(JsonSerializerDefaults.Web);
}