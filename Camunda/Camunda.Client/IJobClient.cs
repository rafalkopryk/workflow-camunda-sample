namespace Camunda.Client;

public interface IJobClient
{
    Task CompleteJobCommand(IJob activatedJob, string variables);

    Task FailCommand(long jobKey, string errorMessage, string variables);

    Task ThrowErrorCommand(long jobKey, string errorCode, string errorMessage, string variables = null);
}

public interface IMessageClient
{
    Task Publish<T>(string correlationKey, T message, string? messageId = null);
}