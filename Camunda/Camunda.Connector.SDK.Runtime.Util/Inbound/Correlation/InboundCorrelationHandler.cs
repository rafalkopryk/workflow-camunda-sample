using Camunda.Connector.SDK.Core.Api.Error;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Camunda.Connector.SDK.Core.Impl.Inbound.Result;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using GatewayProtocol;
using Microsoft.Extensions.Logging;
using static Camunda.Connector.SDK.Core.Impl.Constants;

namespace Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;

public class InboundCorrelationHandler
{
    private readonly ILogger _logger;
    private readonly Gateway.GatewayClient _client;

    public InboundCorrelationHandler(ILogger<InboundCorrelationHandler> logger, Gateway.GatewayClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IInboundConnectorResult<IResponseData>> Correlate(
        InboundConnectorProperties properties, object variables)
    {
        var correlationPoint = properties.CorrelationPoint;
        return correlationPoint switch
        {
            StartEventCorrelationPoint startEventCorrelationPoint => await TriggerStartEvent(startEventCorrelationPoint, variables, properties),
            MessageCorrelationPoint startEventCorrelationPoint => await TriggerMessage(startEventCorrelationPoint, variables, properties),
            _ => throw new ConnectorException("Process correlation point " + correlationPoint.GetType().Name + " is not supported by Runtime")
        }; 
    }

    private async Task<IInboundConnectorResult<ProcessInstance>> TriggerStartEvent(
        StartEventCorrelationPoint correlationPoint, object variables, InboundConnectorProperties properties)
    {
        if (!IsActivationConditionMet(properties, variables))
        {
            _logger.LogDebug("Activation condition didn't match: {correlationPoint.Id}", correlationPoint.GetId());
            return new StartEventCorrelationResult(
                correlationPoint.ProcessDefinitionKey,
                new CorrelationErrorData(CorrelationErrorReason.ACTIVATION_CONDITION_NOT_MET));
        }

        var extractedVariables = ConnectorHelper.CreateOutputVariablesAsString(variables, properties.Properties);

        try
        {
            var result = await _client.CreateProcessInstanceAsync(new CreateProcessInstanceRequest
            {
                BpmnProcessId = correlationPoint.BpmnProcessId,
                Version = correlationPoint.Version,
                Variables = extractedVariables,
            });

            _logger.LogInformation("Created a process instance with key: {response.ProcessInstanceKey}", result.ProcessInstanceKey);
            return new StartEventCorrelationResult(
                result.ProcessDefinitionKey,
                new ProcessInstance(result.ProcessInstanceKey, correlationPoint.BpmnProcessId, correlationPoint.ProcessDefinitionKey, correlationPoint.Version));
        }
        catch (Exception e)
        {
            throw new ConnectorException(
                "Failed to start process instance via StartEvent: " + correlationPoint, e);
        }
    }

    private async Task<IInboundConnectorResult<CorrelatedMessage>> TriggerMessage(
        MessageCorrelationPoint correlationPoint, object variables, InboundConnectorProperties properties)
    {
        var correlationKey = ExtractCorrelationKey(properties, variables);

        if (!IsActivationConditionMet(properties, variables))
        {
            _logger.LogDebug("Activation condition didn't match: {correlationPoint.Id}", correlationPoint.GetId());
            return new MessageCorrelationResult(
                correlationPoint.MessageName,
                new CorrelationErrorData(CorrelationErrorReason.ACTIVATION_CONDITION_NOT_MET));
        }

        var extractedVariables = ConnectorHelper.CreateOutputVariablesAsString(variables, properties.Properties);

        try
        {
            var response = await _client.PublishMessageAsync(new PublishMessageRequest
            {
                CorrelationKey = correlationKey,
                Name = correlationPoint.MessageName,
                Variables = extractedVariables,
                TimeToLive = (long)TimeSpan.FromHours(1).TotalMilliseconds,
                //MessageId = correlationPoint.GetId(),
            });

            _logger.LogInformation("Published message with key: {response.Key}", response.Key);
            return new MessageCorrelationResult(correlationPoint.MessageName, response.Key);
        }
        catch (Exception e)
        {
            throw new ConnectorException(
                "Failed to publish process message for subscription: " + correlationPoint, e);
        }
    }

    private bool IsActivationConditionMet(InboundConnectorProperties properties, object context)
    {
        var activationCondition = properties.GetProperty(ACTIVATION_CONDITION_KEYWORD);
        if (string.IsNullOrWhiteSpace(activationCondition))
        {
            _logger.LogDebug("No activation condition specified for {CorrelationPointId}", properties.GetCorrelationPointId());
            return true;
        }

        //TODO
        //Object shouldActivate = feelEngine.evaluate(activationCondition, context);
        return true;
    }

    private string ExtractCorrelationKey(InboundConnectorProperties properties, object context)
    {
        var correlationKeyExpression = properties.GetRequiredProperty(CORRELATION_KEY_EXPRESSION_KEYWORD);
        try
        {
            return FeelEngineWrapper.EvaluateToJson(correlationKeyExpression, context);
        }
        catch (Exception e)
        {
            throw new ConnectorException(
                "Failed to evaluate correlation key expression: " + correlationKeyExpression, e);
        }
    }
}

