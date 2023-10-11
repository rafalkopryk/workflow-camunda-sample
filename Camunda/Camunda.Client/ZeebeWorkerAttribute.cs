namespace Camunda.Client;

[AttributeUsage(AttributeTargets.Class)]
public class ZeebeWorkerAttribute : Attribute
{
    public string Type { get; set; }
    public long TimeoutInMs { get; set; } = 60_000;
    public string[] FetchVariabeles { get; set; } = Array.Empty<string>();
    public int[] RetryBackOffInMs { get; set; } = new[] { 1000 };
    public bool AutoComplate { get; set; } = true;
    public string[] TenatIds { get; init; } = Array.Empty<string>();
}

public record ServiceTaskConfiguration
{
    public string Type { get; init; }
    public long TimeoutInMs { get; init; } = 60_000;
    public string[] FetchVariabeles { get; init; } = Array.Empty<string>();
    public int[] RetryBackOffInMs { get; init; } = new[] { 1000 };
    public bool AutoComplate { get; init; } = true;
    public string[] TenatIds { get; init; } = Array.Empty<string>();
}

