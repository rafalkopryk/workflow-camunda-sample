using Camunda.Client;
using JsonCons.JmesPath;
using JsonCons.JsonPath;
using System.Text.Json;

namespace Camunda.Connector.SDK.Runtime.Util.Feel;

public static class FeelEngineWrapper
{
    static string RESPONSE_MAP_KEY = "response";
    static string ERROR_CONTEXT_IS_NULL = "Context is null";

    private static string TrimExpression(string expression)
    {
        var feelExpression = expression.Trim();
        if (feelExpression.StartsWith("="))
        {
            feelExpression = feelExpression.Substring(1);
        }
        return feelExpression.Trim();
    }

    /**
     * Evaluates an expression with the FEEL engine with the given variables.
     *
     * @param expression the expression to evaluate
     * @param variables the variables to use in evaluation
     * @return the evaluation result
     * @param <T> the type to cast the evaluation result to
     * @throws FeelEngineWrapperException when there is an exception message as a result of the
     *     evaluation or the result cannot be cast to the given type
     */
    public static string Evaluate(string expression, string variables)
    {
        try
        {
            return EvaluateInternal(expression, variables).RootElement.ToString();
        }
        catch (Exception e)
        {
            throw;
            //throw new FeelEngineWrapperException(e.getMessage(), expression, variables, e);
        }
    }

    /**
     * Evaluates an expression to a JSON String.
     *
     * @param expression the expression to evaluate
     * @param variables the variables to use in evaluation
     * @return the JSON String representing the evaluation result
     * @throws FeelEngineWrapperException when there is an exception message as a result of the
     *     evaluation or the result cannot be parsed as JSON
     */
    public static string EvaluateToJson(string expression, object variables)
    {
        //try
        //{
            var input = ResultToJson(variables);
            var jsonDocument = EvaluateInternal(expression, input);
            return jsonDocument.RootElement.ToString();
        //}
        //catch (Exception e)
        //{
        //    throw new FeelEngineWrapperException(e.getMessage(), expression, variables, e);
        //}
    }

    private static JsonDocument EvaluateInternal(string expression, string variables)
    {
        var doc = JsonDocument.Parse(variables);
        var path = TrimExpression(expression);
        return JsonTransformer.Transform(doc.RootElement, path);
    }

    public static string ResultToJson(object result)
    {
        return JsonSerializer.Serialize(result, JsonSerializerCustomOptions.CamelCase);
    }
}
