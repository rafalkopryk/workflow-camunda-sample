namespace Camunda.Client;

[AttributeUsage(AttributeTargets.Class)]
public class ZeebeWorkerAttribute : Attribute
{
    public string Type { get; set; }
    public int MaxJobsToActivate { get; set; } = 5;
    public int TimeoutInMs { get; set; } = 10_000;
    public int PollIntervalInMs { get; set; } = 500;
    public int RequestTimeoutInMs { get; set; } = 10_000;
    public string[] FetchVariabeles { get; set; } = Array.Empty<string>();
    public int[] RetryBackOffInMs { get; set; } = new[] { 1000 };
    public bool AutoComplate { get; set; } = true;
}

public record ServiceTaskConfiguration
{
    public string Type { get; init; }
    public int MaxJobsToActivate { get; init; } = 5;
    public int TimeoutInMs { get; init; } = 10_000;
    public int PollIntervalInMs { get; init; } = 500;
    public int RequestTimeoutInMs { get; init; } = 10_000;
    public string[] FetchVariabeles { get; init; } = Array.Empty<string>();
    public int[] RetryBackOffInMs { get; init; } = new[] { 1000 };
    public bool AutoComplate { get; init; } = true;
}

