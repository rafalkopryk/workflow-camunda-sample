namespace Camunda.Client.Jobs;

[AttributeUsage(AttributeTargets.Class)]
public class JobWorkerAttribute : Attribute
{
    public string Type { get; init; }
    public string[] FetchVariabeles { get; init; } = [];
}

public record JobWorkerConfiguration
{
    public long TimeoutInMs { get; init; } = 60_000;
    public int[] RetryBackOffInMs { get; init; } = [1000];
    public bool AutoComplete { get; init; } = true;
    public string[] TenatIds { get; init; } = [];
    public int PoolingMaxJobsToActivate { get; init; } = 20;
    public long PoolingRequestTimeoutInMs { get; set; } = 20_000;
    public int PoolingDelayInMs { get; init; } = 100;
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } = 900;
}

internal record InternalJobWorkerConfiguration
{
    public string Type { get; init; }
    public string[] FetchVariabeles { get; init; } = [];
    public long TimeoutInMs { get; init; }
    public int[] RetryBackOffInMs { get; init; } = [];
    public bool AutoComplete { get; init; }
    public string[] TenatIds { get; init; } = [];
    public int PoolingMaxJobsToActivate { get; init; }
    public long PoolingRequestTimeoutInMs { get; set; } 
    public int PoolingDelayInMs { get; init; }
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } 
}