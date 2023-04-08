namespace Camunda.Client;
using System.Diagnostics;

internal static class Diagnostics
{
    private const string ActivitySourceName = "Camunda.Client";
    public static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);

    internal static class Consumer
    {
        internal static Activity Start(string jobType)
        {
            var activityName = $"ZeebeWorker ActivateJobs from {jobType}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Consumer);
        }
    }

    //public static Activity AddDefaultOpenTelemetryTags<TKey, TValue>(
    //    this Activity activity,
    //    string topic,
    //    Message<TKey, TValue> message)
    //{
    //    activity?.AddTag(MessagingAttributes.SYSTEM, "zeebe");

    //    activity?.AddTag(MessagingAttributes.DESTINATION, topic);
    //    activity?.AddTag(MessagingAttributes.DESTINATION_KIND, "topic");

    //    return activity;
    //}
}
