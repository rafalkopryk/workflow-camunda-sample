using Camunda.Client;
using DevLab.JmesPath;
using JsonCons.JmesPath;
using System.Text.Json;

namespace Camunda.Connector.SDK.Runtime.Util.Feel;

public static class FeelEngineWrapper
{
    static string RESPONSE_MAP_KEY = "response";
    static string ERROR_CONTEXT_IS_NULL = "Context is null";

    public static string? Evaluate(string expression, object variables, IJsonTransformerEngine jsonTransformerEngine)
    {
        var path = TrimExpression(expression);
        var input = JsonSerializer.Serialize(variables, JsonSerializerCustomOptions.CamelCase);
        return jsonTransformerEngine.Transform(path, input);
    }

    private static string TrimExpression(string expression)
    {
        var feelExpression = expression.Trim();
        if (feelExpression.StartsWith("="))
        {
            feelExpression = feelExpression.Substring(1);
        }

        return feelExpression.Trim();
    }
}

public interface IJsonTransformerEngine
{
    string? Transform(string jmesPath, string json);
}

public class ConsJsonTransformerEngine : IJsonTransformerEngine
{
    public string? Transform(string jmesPath, string json)
    {
        var result = JsonTransformer.Transform(JsonDocument.Parse(json).RootElement, jmesPath).RootElement;
        return result.ValueKind == JsonValueKind.Null
            ? null
            : result.ToString();
    }
}

public class JmesPathJsonTransformerEngine : IJsonTransformerEngine
{
    const string NULL_KEYWORD = "null";
    private static readonly JmesPath s_JmesPath = new JmesPath();

    public string? Transform(string jmesPath, string json)
    {
        var result = s_JmesPath.Transform(json, jmesPath);
        return string.Equals(result, NULL_KEYWORD, StringComparison.OrdinalIgnoreCase)
            ? null
            : result;
    }
}
