﻿namespace Common.Application.Zeebe;

[AttributeUsage(AttributeTargets.Class)]
public class ZeebeTaskAttribute : Attribute
{
    public string Type { get; set; }
    public int MaxJobsToActivate { get; set; } = 5;
    public int TimeoutInMs { get; set; } = 10_000;
    public int PollIntervalInMs { get; set; } = 500;
    public int PollingTimeoutInMs { get; set; } = 10_000;
    public string[] FetchVariabeles { get; set; } = Array.Empty<string>();
    public int[] RetryBackOffInMs { get; set; } = new[] { 1000 };
}
