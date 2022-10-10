using System;

namespace Common.Application.Zeebe
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ZeebeJobAttribute : Attribute
    {
        public string JobType { get; set; }
        public int? MaxJobsActive { get; set; }
        public TimeSpan? Timeout { get; set; }
        public TimeSpan? PollInterval { get; set; }
        public TimeSpan? PollingTimeout { get; set; }
        public string[] FetchVariabeles { get; set; }
    }
}
