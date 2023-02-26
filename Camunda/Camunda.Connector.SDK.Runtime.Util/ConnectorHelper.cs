namespace Camunda.Connector.SDK.Runtime.Util;

public class ConnectorHelper
{
    //TODO
    //public static FeelEngineWrapper FEEL_ENGINE_WRAPPER = new FeelEngineWrapper();
    //// TODO: Check if this is properly configured
    //public static ObjectMapper OBJECT_MAPPER = new ObjectMapper();

    private static string ERROR_CANNOT_PARSE_VARIABLES = "Cannot parse '%s' as '%s'.";

    public const string RESULT_VARIABLE_HEADER_NAME = "resultVariable";
    public const string RESULT_EXPRESSION_HEADER_NAME = "resultExpression";
    public const string ERROR_EXPRESSION_HEADER_NAME = "errorExpression";

  public static Dictionary<string, object> CreateOutputVariables(object responseContent, Dictionary<string, string> jobHeaders)
  {
        Dictionary<string, object> outputVariables = new ();
        var resultVariableName = jobHeaders.FirstOrDefault(x => x.Key == RESULT_VARIABLE_HEADER_NAME).Value;
        var resultExpression = jobHeaders.FirstOrDefault(x => x.Key == RESULT_EXPRESSION_HEADER_NAME).Value;

        if (!string.IsNullOrWhiteSpace(resultVariableName))
        {
            outputVariables.Add(resultVariableName, responseContent);
        }

        //TODO
        //Optional.ofNullable(resultExpression)
        //    .filter(s-> !s.isBlank())
        //        .map(expression->FEEL_ENGINE_WRAPPER.evaluateToJson(expression, responseContent))
        //        .map(json->parseJsonVarsAsTypeOrThrow(json, Map.class, resultExpression))
        //    .ifPresent(outputVariables::putAll);

        return outputVariables;
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