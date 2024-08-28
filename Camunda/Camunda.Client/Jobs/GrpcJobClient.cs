using GatewayProtocol;

namespace Camunda.Client.Jobs;

internal class GrpcJobClient(Gateway.GatewayClient client) : IJobClient
{
    private readonly Gateway.GatewayClient _client = client;

    public async Task CompleteJobCommand(IJob activatedJob, string? variables = null)
    {
        await _client.CompleteJobAsync(new CompleteJobRequest
        {
            JobKey = activatedJob.Key,
            Variables = variables ?? string.Empty
        });
    }

    public async Task FailCommand(long jobKey, string errorMessage, int retries, long retryBackOff, string? variables = null)
    {
        await _client.FailJobAsync(new FailJobRequest
        {
            JobKey = jobKey,
            ErrorMessage = errorMessage,
            Variables = variables ?? string.Empty,
            Retries = retries,
            RetryBackOff = retryBackOff
        });
    }

    public async Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage, string? variables = null)
    {
        await _client.ThrowErrorAsync(new ThrowErrorRequest
        {
            JobKey = jobKey,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Variables = variables ?? string.Empty,
        });
    }
}
