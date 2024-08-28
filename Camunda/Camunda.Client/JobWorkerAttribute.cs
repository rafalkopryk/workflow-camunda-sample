namespace Camunda.Client;


[AttributeUsage(AttributeTargets.Class)]
public class JobWorkerAttribute : Attribute
{
    public string Type { get; init; }
    public long TimeoutInMs { get; init; } = 60_000;
    public string[] FetchVariabeles { get; init; } = [];
    public int[] RetryBackOffInMs { get; init; } = [1000];
    public bool AutoComplete { get; init; } = true;
    public string[] TenatIds { get; init; } = [];
    public int PoolingMaxJobsToActivate { get; init; } = 20;
    public long PoolingRequestTimeoutInMs { get; set; } = 20_000;
    public int PoolingDelayInMs { get; init; } = 100;
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } = 900;
}

public record JobWorkerConfiguration
{
    public string? Type { get; init; }
    public long TimeoutInMs { get; init; } = 60_000;
    public string[] FetchVariabeles { get; init; } = [];
    public int[] RetryBackOffInMs { get; init; } = [1000];
    public bool AutoComplete { get; init; } = true;
    public string[] TenatIds { get; init; } = [];
    public int PoolingMaxJobsToActivate { get; init; } = 20;
    public long PoolingRequestTimeoutInMs { get; set; } = 20_000;
    public int PoolingDelayInMs { get; init; } = 100;
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } = 900;
}

