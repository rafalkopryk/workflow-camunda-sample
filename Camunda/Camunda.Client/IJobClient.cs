namespace Camunda.Client;

public interface IJobClient
{
    Task CompleteJobCommand(IJob activatedJob, string variables);

    Task FailCommand(long jobKey, string errorMessage);

    Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage);
}