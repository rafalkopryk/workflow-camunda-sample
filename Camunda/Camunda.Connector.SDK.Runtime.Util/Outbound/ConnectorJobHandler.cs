using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Error;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Camunda.Connector.SDK.Runtime.Util.Outbound;

[ZeebeWorker(AutoComplate = false)]
public class ConnectorJobHandler : IJobHandler
{
    private readonly ILogger _logger;

    // Protects Zeebe from enormously large messages it cannot handle
    public static int MAX_ERROR_MESSAGE_LENGTH = 6000;

    protected IOutboundConnectorFunction _call;
    //protected SecretProvider secretProvider;

    public ConnectorJobHandler(IOutboundConnectorFunction call, ILogger<ConnectorJobHandler> logger)
    {
        _call = call;
        _logger = logger;
    }

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received job {job.Key}", job.Key);

        var result = new ConnectorResult();
        try
        {
            result.ResponseValue = _call.Execute(new JobHandlerContext(job));

            var customHeaders = JsonSerializer.Deserialize<Dictionary<string, string>>(job.CustomHeaders);
            result.Variables = ConnectorHelper.CreateOutputVariables(result.GetResponseValue, customHeaders);
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Exception while processing job {job.Key}, error: {ex}", job.Key, ex);
            result.ResponseValue = new
            {
                Error = ToMap(ex),
            };

            result.Exception = ex;
        }

        try
        {
            //ConnectorHelper.examineErrorExpression(result.getResponseValue(), job.getCustomHeaders())
            //.ifPresentOrElse(
            //        error-> {
            //    LOGGER.debug(
            //        "Throwing BPMN error for job {} with code {}", job.getKey(), error.getCode());
            //    throwBpmnError(client, job, error);
            //},
            //  ()-> {
            //    if (result.isSuccess())
            //    {
            //        LOGGER.debug("Completing job {}", job.getKey());
            //        completeJob(client, job, result);
            //    }
            //    else
            //    {
            //        logError(job, result.getException());
            //        failJob(client, job, result.getException());
            //    }
            //});

            if (result.IsSuccess)
            {
                _logger.LogDebug("Completing job {job.Key}", job.Key);
                await CompleteJob(client, job, result);
            }
            else
            {
                LogError(job, result.Exception);
                await FailJob(client, job, result.Exception);
            }
        }
        catch (Exception ex)
        {
            LogError(job, ex);
            await FailJob(client, job, ex);
        }
    }

    protected void LogError(IJob job, Exception ex)
    {
        _logger.LogError("Exception while processing job {job.Key}, error: {ex}", job.Key, ex);
    }

    protected async Task CompleteJob(IJobClient client, IJob job, ConnectorResult result)
    {
        var variables = JsonSerializer.Serialize(result.Variables);
        await client.CompleteJobCommand(job, variables);
    }

    protected async Task FailJob(IJobClient client, IJob job, Exception exception)
    {
        var message = exception.Message;
        var truncatedMessage = message?.Substring(0, Math.Min(message.Length, MAX_ERROR_MESSAGE_LENGTH));
        await client.FailCommand(job.Key,truncatedMessage);
    }

    protected async Task ThrowBpmnError(IJobClient client, IJob job, BpmnError value)
    {
        var message = value.Message;
        var truncatedMessage = message?.Substring(0, Math.Min(message.Length, MAX_ERROR_MESSAGE_LENGTH));

        await client
            .ThrowErrorCommand(job.Key, value.Code, truncatedMessage);
    }

    protected static Dictionary<string, object> ToMap(Exception exception)
    {
        Dictionary<string, object> result = new()
        {
            { "type", exception.GetType().Name }
        };

        var message = exception.Message;
        if (message != null)
        {
            result.Add("message", message.Substring(0, Math.Min(message.Length, MAX_ERROR_MESSAGE_LENGTH)));
        }

        if (exception is ConnectorException connectorException)
        {
            var code = connectorException.ErrorCode;
            if (code != null)
            {
                result.Add("code", code);
            }
        }
        return result;
    }
}
