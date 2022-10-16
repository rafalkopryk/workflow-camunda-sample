﻿using System;

namespace Common.Application.Zeebe
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ZeebeJobAttribute : Attribute
    {
        public string JobType { get; set; }
        public int MaxJobsActive { get; set; } = 1;
        public int TimeoutInMs { get; set; } = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
        public int PollIntervalInMs { get; set; } = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;
        public int PollingTimeoutInMs { get; set; } = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
        public string[] FetchVariabeles { get; set; } = Array.Empty<string>();
    }
}
