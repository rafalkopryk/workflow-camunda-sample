namespace Camunda.Client.Jobs;

using Camunda.Client.Rest;

internal class RestJobClient(ICamundaClientRest client) : IJobClient
{
    private readonly ICamundaClientRest _client = client;

    public async Task CompleteJobCommand(IJob activatedJob, string? variables = null)
    {
        var jsonAsObject = !string.IsNullOrWhiteSpace(variables)
            ? Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(variables)
            : [];

        await _client.CompletionAsync(activatedJob.Key, new JobCompletionRequest
        {
            Variables = jsonAsObject,
        });
    }

    public async Task FailCommand(long jobKey, string errorMessage, int retries, long retryBackOff, string? variables = null)
    {
        var jsonAsObject = !string.IsNullOrWhiteSpace(variables)
            ? Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(variables)
            : [];

        await _client.FailureAsync(jobKey, new JobFailRequest
        {
            ErrorMessage = errorMessage,
            Variables = jsonAsObject,
            Retries = retries,
            RetryBackOff = retryBackOff,
        });
    }

    public async Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage, string? variables = null)
    {
        var jsonAsObject = !string.IsNullOrWhiteSpace(variables)
            ? Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(variables)
            : [];

        await _client.ErrorAsync(jobKey, new JobErrorRequest
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Variables = jsonAsObject,
        });
    }
}