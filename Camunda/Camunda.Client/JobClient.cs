using GatewayProtocol;

namespace Camunda.Client;

internal class JobClient : IJobClient
{
    private readonly Gateway.GatewayClient _client;

    public JobClient(Gateway.GatewayClient client)
    {
        _client = client;
    }

    public async Task CompleteJobCommand(IJob activatedJob, string variables)
    {
        await _client.CompleteJobAsync(new CompleteJobRequest
        {
            JobKey = activatedJob.Key,
            Variables = variables
        });
    }

    public async Task FailCommand(long jobKey, string errorMessage)
    {
        await _client.FailJobAsync(new FailJobRequest
        {
            JobKey = jobKey,
            ErrorMessage = errorMessage,
        });
    }

    public async Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage)
    {
        await _client.ThrowErrorAsync(new ThrowErrorRequest
        {
            JobKey = jobKey,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        });
    }
}
