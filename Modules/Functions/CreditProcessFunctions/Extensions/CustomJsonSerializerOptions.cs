using System.Text.Json;

namespace CreditProcessFunctions;

public class CustomJsonSerializerOptions
{
    public readonly static JsonSerializerOptions CamelCase = new(JsonSerializerDefaults.Web);
}

