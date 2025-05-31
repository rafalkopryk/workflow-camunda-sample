namespace Camunda.Client.Jobs;

[AttributeUsage(AttributeTargets.Class)]
public class JobWorkerAttribute : Attribute
{
    public string Type { get; init; }
    public string[] FetchVariables { get; init; } = [];
}

public record JobWorkerConfiguration
{
    public long TimeoutInMs { get; init; } = 60_000;
    public int[] RetryBackOffInMs { get; init; } = [1000];
    public bool AutoComplete { get; init; } = true;
    public string[] TenantIds { get; init; } = [];
    public int PollingMaxJobsToActivate { get; init; } = 20;
    public long PollingRequestTimeoutInMs { get; set; } = 20_000;
    public int PollingDelayInMs { get; init; } = 200;
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } = 900;
}

internal record InternalJobWorkerConfiguration
{
    public string Type { get; init; }
    public string[] FetchVariables { get; init; } = [];
    public long TimeoutInMs { get; init; }
    public int[] RetryBackOffInMs { get; init; } = [];
    public bool AutoComplete { get; init; }
    public string[] TenantIds { get; init; } = [];
    public int PollingMaxJobsToActivate { get; init; }
    public long PollingRequestTimeoutInMs { get; set; } 
    public int PollingDelayInMs { get; init; }
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } 
}