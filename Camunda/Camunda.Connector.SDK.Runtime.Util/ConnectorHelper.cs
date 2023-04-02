using Camunda.Client;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using System.Linq.Expressions;
using System.Text.Json;

using static Camunda.Connector.SDK.Core.Impl.Constants;

namespace Camunda.Connector.SDK.Runtime.Util;

public class ConnectorHelper
{
    //TODO
    //public static FeelEngineWrapper FEEL_ENGINE_WRAPPER = new FeelEngineWrapper();
    //// TODO: Check if this is properly configured
    //public static ObjectMapper OBJECT_MAPPER = new ObjectMapper();

    private static string ERROR_CANNOT_PARSE_VARIABLES = "Cannot parse '%s' as '%s'.";

    public static Dictionary<string, object> CreateOutputVariables(object responseContent, Dictionary<string, string> jobHeaders)
    {
        Dictionary<string, object> outputVariables = new();
        var resultVariableName = jobHeaders.FirstOrDefault(x => x.Key == RESULT_VARIABLE_KEYWORD).Value;
        var resultExpression = jobHeaders.FirstOrDefault(x => x.Key == RESULT_EXPRESSION_KEYWORD).Value;

        if (!string.IsNullOrWhiteSpace(resultVariableName))
        {
            outputVariables.Add(resultVariableName, responseContent);
        }

        //TODO

        if (!string.IsNullOrWhiteSpace(resultExpression))
        {
            var json = FeelEngineWrapper.EvaluateToJson(resultExpression, responseContent);
            var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonSerializerCustomOptions.CamelCase);
            foreach (var result in resultDictionary)
            {
                outputVariables.TryAdd(result.Key, result.Value);
            }
        }

        return outputVariables;
    }

    public static string CreateOutputVariablesAsString(object responseContent, Dictionary<string, string> jobHeaders)
    {
        var result = CreateOutputVariables(responseContent, jobHeaders);
        return JsonSerializer.Serialize(result, JsonSerializerCustomOptions.CamelCase);
    }

    //public static Optional<BpmnError> examineErrorExpression(
    //    final Object responseContent, final Map<String, String> jobHeaders)
    //{
    //    final var errorExpression = jobHeaders.get(ERROR_EXPRESSION_HEADER_NAME);
    //    return Optional.ofNullable(errorExpression)
    //    .filter(s-> !s.isBlank())
    //        .map(expression->FEEL_ENGINE_WRAPPER.evaluateToJson(expression, responseContent))
    //        .map(json->parseJsonVarsAsTypeOrThrow(json, BpmnError.class, errorExpression))
    //        .filter(BpmnError::hasCode);
    //  }
}