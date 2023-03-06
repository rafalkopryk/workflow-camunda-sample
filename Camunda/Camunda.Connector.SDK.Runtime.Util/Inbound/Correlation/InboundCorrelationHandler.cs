using Camunda.Connector.SDK.Core.Api.Error;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Camunda.Connector.SDK.Runtime.Util.Feel;
using GatewayProtocol;
using Microsoft.Extensions.Logging;

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

    public async Task<InboundConnectorResult> Correlate(
        ProcessCorrelationPoint correlationPoint, string variables)
    {
        return correlationPoint switch
        {
            StartEventCorrelationPoint startEventCorrelationPoint => await TriggerStartEvent(startEventCorrelationPoint, variables),
            MessageCorrelationPoint startEventCorrelationPoint => await TriggerMessage(startEventCorrelationPoint, variables),
            _ => throw new ConnectorException("Process correlation point " + correlationPoint.GetType().Name + " is not supported by Runtime")
        };
    }

    private async Task<InboundConnectorResult> TriggerStartEvent(
        StartEventCorrelationPoint correlationPoint, string variables)
    {
        try
        {
            var result = await _client.CreateProcessInstanceAsync(new CreateProcessInstanceRequest
            {
                BpmnProcessId = correlationPoint.BpmnProcessId,
                Version = correlationPoint.Version,
                Variables = variables,
            });

            _logger.LogInformation("Created a process instance with key: {response.ProcessInstanceKey}", result.ProcessInstanceKey);
            return new StartEventInboundConnectorResult(result);

        }
        catch (Exception e)
        {
            throw new ConnectorException(
                "Failed to start process instance via StartEvent: " + correlationPoint, e);
        }
    }

    private async Task<InboundConnectorResult> TriggerMessage(
        MessageCorrelationPoint correlationPoint, string variables)
    {
        var correlationKey = FeelEngineWrapper.Evaluate(correlationPoint.CorrelationKeyExpression, variables);

        try
        {
            var response = await _client.PublishMessageAsync(new PublishMessageRequest
            {
                CorrelationKey = correlationKey,
                Name = correlationPoint.MessageName,
                Variables = variables,
                TimeToLive = (long)TimeSpan.FromHours(1).TotalMilliseconds,
                //MessageId = correlationPoint.GetId(),
            });

            _logger.LogInformation("Published message with key: {response.Key}", +response.Key);
            return new MessageInboundConnectorResult(response, correlationKey);
        }
        catch (Exception e)
        {
            throw new ConnectorException(
                "Failed to publish process message for subscription: " + correlationPoint, e);
        }
    }
}

