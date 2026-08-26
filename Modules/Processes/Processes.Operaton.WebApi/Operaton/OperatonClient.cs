using System.Text.Json;
using Processes.Operaton.WebApi.Operaton.Generated;

namespace Processes.Operaton.WebApi.Operaton;

public sealed class OperatonClient(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "Operaton";

    private readonly IOperatonApiClient _client = new OperatonApiClient(
        httpClientFactory.CreateClient(HttpClientName));

    public async Task DeployAsync(string name, byte[] bpmn, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(bpmn);
        await _client.CreateDeploymentAsync(
            tenant_id: null,
            deployment_source: null,
            deploy_changed_only: true,
            enable_duplicate_filtering: true,
            deployment_name: name,
            deployment_activation_time: null,
            data: new FileParameter(stream, $"{name}.bpmn", "application/xml"),
            cancellationToken: cancellationToken);
    }

    public async Task CorrelateMessageAsync(
        string messageName,
        string businessKey,
        object variables,
        CancellationToken cancellationToken = default,
        bool correlateAll = false)
    {
        try
        {
            await _client.DeliverMessageAsync(new CorrelationMessageDto
            {
                MessageName = messageName,
                BusinessKey = businessKey,
                All = correlateAll,
                ResultEnabled = false,
                ProcessVariables = ToVariables(variables),
            }, cancellationToken);
        }
        catch (ApiException exception) when (exception.StatusCode == StatusCodes.Status204NoContent)
        {
            // The official specification models the successful result-disabled response as a distinct 204 response.
        }
        catch (ApiException<ExceptionDto> exception) when (exception.StatusCode == StatusCodes.Status400BadRequest)
        {
            throw new InvalidOperationException(
                $"Operaton could not correlate message '{messageName}' for business key '{businessKey}': " +
                (exception.Result.Message ?? "unknown correlation error"),
                exception);
        }
    }

    public async Task<IReadOnlyList<OperatonExternalTask>> FetchAndLockAsync(
        string workerId,
        IReadOnlyCollection<IOperatonJobHandler> handlers,
        CancellationToken cancellationToken)
    {
        var tasks = await _client.FetchAndLockAsync(new FetchExternalTasksDto
        {
            WorkerId = workerId,
            MaxTasks = Math.Max(1, handlers.Count * 2),
            UsePriority = true,
            AsyncResponseTimeout = 10_000,
            Topics = handlers.Select(x => new FetchExternalTaskTopicDto
            {
                TopicName = x.Topic,
                LockDuration = (long)x.LockDuration.TotalMilliseconds,
            }).ToArray(),
        }, cancellationToken);

        return tasks.Select(ToExternalTask).ToArray();
    }

    public Task CompleteAsync(string taskId, string workerId, CancellationToken cancellationToken) =>
        _client.CompleteExternalTaskResourceAsync(
            taskId,
            new CompleteExternalTaskDto { WorkerId = workerId },
            cancellationToken);

    public Task ReportFailureAsync(
        OperatonExternalTask task,
        string workerId,
        Exception exception,
        CancellationToken cancellationToken) =>
        _client.HandleFailureAsync(
            task.Id,
            new ExternalTaskFailureDto
            {
                WorkerId = workerId,
                ErrorMessage = exception.Message,
                ErrorDetails = exception.ToString(),
                Retries = Math.Max(0, (task.Retries ?? 3) - 1),
                RetryTimeout = 10_000,
            },
            cancellationToken);

    private static Dictionary<string, VariableValueDto> ToVariables(object variables)
    {
        var values = JsonSerializer.SerializeToElement(variables, JsonSerializerOptions.Web);
        return values.EnumerateObject().ToDictionary(
            property => property.Name,
            property => new VariableValueDto
            {
                Value = new AnyValue { JsonValue = property.Value },
            });
    }

    private static OperatonExternalTask ToExternalTask(LockedExternalTaskDto task) => new(
        task.Id ?? throw MissingTaskProperty(nameof(task.Id)),
        task.TopicName ?? throw MissingTaskProperty(nameof(task.TopicName)),
        task.ProcessInstanceId ?? throw MissingTaskProperty(nameof(task.ProcessInstanceId)),
        task.BusinessKey,
        task.Retries,
        (task.Variables ?? new Dictionary<string, VariableValueDto>()).ToDictionary(
            x => x.Key,
            x => (object)x.Value.Value.JsonValue));

    private static InvalidOperationException MissingTaskProperty(string propertyName) =>
        new($"Operaton external task response does not contain {propertyName}.");
}
