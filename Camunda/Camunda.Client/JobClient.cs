using GatewayProtocol;

namespace Camunda.Client;

internal class JobClient(Gateway.GatewayClient client) : IJobClient
{
    private readonly Gateway.GatewayClient _client = client;

    public async Task CompleteJobCommand(IJob activatedJob, string variables)
    {
        await _client.CompleteJobAsync(new CompleteJobRequest
        {
            JobKey = activatedJob.Key,
            Variables = variables
        });
    }

    public async Task FailCommand(long jobKey, string errorMessage, string veriables)
    {
        await _client.FailJobAsync(new FailJobRequest
        {
            JobKey = jobKey,
            ErrorMessage = errorMessage,
            Variables = veriables
        });
    }

    public async Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage, string variables)
    {
        await _client.ThrowErrorAsync(new ThrowErrorRequest
        {
            JobKey = jobKey,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Variables= variables
        });
    }
}
