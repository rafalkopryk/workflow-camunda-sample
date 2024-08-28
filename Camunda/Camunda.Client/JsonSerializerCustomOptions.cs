using System.Text.Json;
using System.Text.Json.Serialization;

namespace Camunda.Client;

public static class JsonSerializerCustomOptions
{
    public static readonly JsonSerializerOptions CamelCase = new(JsonSerializerDefaults.Web);
}