namespace Camunda.Client;
using System.Diagnostics;
using System.Diagnostics.Metrics;

internal static class Diagnostics
{
    private const string ActivitySourceName = "Camunda.Client";
    public static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);

    public static readonly Meter Meter = new("Camunda.Client.Metric");
    public static readonly Counter<long> StreamMessagesCounter = Meter.CreateCounter<long>("stream-messages.count");

    internal static class Consumer
    {
        internal static Activity StartStreamActivatedJobs(string jobType)
        {
            var activityName = $"ZeebeWorker JobStream ACTIVATED from {jobType}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Consumer);
        }

        internal static Activity StartHandleTask(string elementId)
        {
            var activityName = $"ZeebeWorker Task RECEIVE from {elementId}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Consumer);
        }
    }

    public static Activity AddDefaultOpenTelemetryTags<TKey, TValue>(
        this Activity activity,
        string job)
    {
        activity?.AddTag(MessagingAttributes.SYSTEM, "zeebe");

        activity?.AddTag(MessagingAttributes.DESTINATION, job);
        activity?.AddTag(MessagingAttributes.DESTINATION_KIND, "topic");

        return activity;
    }
}
