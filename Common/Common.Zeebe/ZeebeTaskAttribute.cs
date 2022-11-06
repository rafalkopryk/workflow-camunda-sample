namespace Common.Application.Zeebe;

[AttributeUsage(AttributeTargets.Class)]
public class ZeebeTaskAttribute : Attribute
{
    public string Type { get; set; }
    public int MaxJobsToActivate { get; set; } = 5;
    public int TimeoutInMs { get; set; } = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
    public int PollIntervalInMs { get; set; } = (int)TimeSpan.FromMilliseconds(500).TotalMilliseconds;
    public int PollingTimeoutInMs { get; set; } = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
    public string[] FetchVariabeles { get; set; } = Array.Empty<string>();
}
