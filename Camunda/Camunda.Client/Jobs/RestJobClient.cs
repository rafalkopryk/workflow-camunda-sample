namespace Camunda.Client.Jobs;

using Camunda.Client.Rest;

internal class RestJobClient(CamundaClientRest client) : IJobClient
{
    private readonly CamundaClientRest _client = client;

    public async Task CompleteJobCommand(IJob activatedJob, string? variables = null)
    {
        await _client.CompletionAsync(activatedJob.Key, new JobCompletionRequest
        {
            Variables = variables ?? string.Empty,
        });
    }

    public async Task FailCommand(long jobKey, string errorMessage, int retries, long retryBackOff, string? variables = null)
    {
        await _client.FailureAsync(jobKey, new JobFailRequest
        {
            ErrorMessage = errorMessage,
            Variables = variables ?? string.Empty,
            Retries = retries,
            RetryBackOff = retryBackOff,
        });
    }

    public async Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage, string? variables = null)
    {
        await _client.ErrorAsync(jobKey, new JobErrorRequest
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Variables = variables ?? string.Empty,
        });
    }
}