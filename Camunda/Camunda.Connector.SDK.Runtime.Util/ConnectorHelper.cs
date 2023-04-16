using Camunda.Client;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using System.Text.Json;

using static Camunda.Connector.SDK.Core.Impl.Constants;

namespace Camunda.Connector.SDK.Runtime.Util;

public class ConnectorHelper
{
    private static string ERROR_CANNOT_PARSE_VARIABLES = "Cannot parse '%s' as '%s'.";

    public static Dictionary<string, object> CreateOutputVariables(object responseContent, Dictionary<string, string> jobHeaders, IJsonTransformerEngine jsonTransformerEngine)
    {
        Dictionary<string, object> outputVariables = new();
        var resultVariableName = jobHeaders?.FirstOrDefault(x => x.Key == RESULT_VARIABLE_KEYWORD).Value;
        var resultExpression = jobHeaders?.FirstOrDefault(x => x.Key == RESULT_EXPRESSION_KEYWORD).Value;

        if (!string.IsNullOrWhiteSpace(resultVariableName))
        {
            outputVariables.Add(resultVariableName, responseContent);
        }

        //TODO

        if (!string.IsNullOrWhiteSpace(resultExpression))
        {
            var json = FeelEngineWrapper.Evaluate(resultExpression, responseContent, jsonTransformerEngine);
            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonSerializerCustomOptions.CamelCase);
            foreach (var result in resultDictionary)
            {
                outputVariables.TryAdd(result.Key, result.Value);
            }
        }

        return outputVariables;
    }

    public static string CreateOutputVariablesAsString(object responseContent, Dictionary<string, string> jobHeaders, IJsonTransformerEngine jsonTransformerEngine)
    {
        var result = CreateOutputVariables(responseContent, jobHeaders, jsonTransformerEngine);
        return JsonSerializer.Serialize(result, JsonSerializerCustomOptions.CamelCase);
    }
}