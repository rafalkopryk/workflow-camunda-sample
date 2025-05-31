namespace Camunda.Client.Jobs;

public interface IJobClient
{
    Task CompleteJobCommand(IJob activatedJob, string? variables = null);

    Task FailCommand(long jobKey, string errorMessage, int retries, long retryBackOff, string? variables = null);

    Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage, string? variables = null);
}
