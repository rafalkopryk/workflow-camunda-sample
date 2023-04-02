namespace Camunda.Connector.SDK.Core.Impl;

public static class Constants
{
    /**
     * The keyword that identifies the source of `result variable` property of a Connector. Result
     * variable is a property that contains the name of the process variable where the Connector
     * result will be stored.
     *
     * <p>For outbound Connectors, this value comes from Zeebe job headers.
     *
     * <p>For inbound Connectors, this value comes from the extension properties of a BPMN element.
     */
    public static string RESULT_VARIABLE_KEYWORD = "resultVariable";

    /**
     * The keyword that identifies the source of `result expression` property of a Connector. Result
     * expression is a FEEL expression that is used to map the Connector output into process variables
     *
     * <p>For outbound Connectors, this value comes from Zeebe job headers.
     *
     * <p>For inbound Connectors, this value comes from the extension properties of a BPMN element.
     */
    public static string RESULT_EXPRESSION_KEYWORD = "resultExpression";

    /**
     * The keyword that identifies the source of `error expression` property of a Connector. Error
     * expression is a FEEL context expression that is used to map the Connector output into process
     * variables
     *
     * <p>This value only exists for outbound Connectors and comes from Zeebe job headers.
     */
    public static string ERROR_EXPRESSION_KEYWORD = "errorExpression";

    /**
     * The keyword that identifies the source of `correlation key expression` property of a Connector.
     * Correlation key expression is a FEEL expression that is extracts the correlation key from the
     * inbound Connector output.
     *
     * <p>This value only exists for inbound Connectors that target an intermediate message catch
     * event and comes from the extension properties of a BPMN element.
     */
    public static string CORRELATION_KEY_EXPRESSION_KEYWORD = "correlationKeyExpression";

    /**
     * The keyword that identifies the source of `activation condition` property of a Connector.
     * Activation condition is a boolean FEEL expression that determines whether the inbound Connector
     * should be activated based on the inbound payload.
     *
     * <p>This value only exists for inbound Connectors and comes from the extension properties of a
     * BPMN element.
     */
    public static string ACTIVATION_CONDITION_KEYWORD = "activationCondition";

    /**
     * The keyword that identifies the source of `type` property of an inbound Connector. Type
     * identifies the specific inbound Connector implementation.
     */
    public static string INBOUND_TYPE_KEYWORD = "inbound.type";
}

