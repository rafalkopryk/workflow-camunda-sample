namespace Camunda.Client;

using OpenTelemetry.Trace;
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
            var activityName = $"CamundaWorker JobStream ACTIVATED from {jobType}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Consumer);
        }

        internal static Activity StartPoolActivatedJobs(string jobType)
        {
            var activityName = $"CamundaWorker JobPool ACTIVATED from {jobType}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Consumer);
        }

        internal static Activity StartHandleTask(string elementId)
        {
            var activityName = $"CamundaWorker Task RECEIVE from {elementId}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Consumer);
        }
    }

    public static void AddtOpenTelemetryTags(this Activity? activity, IJob job)
    {
        activity?.AddTag(MessagingAttributes.SYSTEM, "camunda");

        activity?.AddTag(MessagingAttributes.DESTINATION_NAME, job.Type);
        activity?.AddTag(MessagingAttributes.DESTINATION, job.Type);

        activity?.AddTag(MessagingAttributes.OPERATION, "receive");
        activity?.AddTag(MessagingAttributes.CAMUNDA_PROCESS_INSTANCE_KEY, job.ProcessInstanceKey);
        activity?.AddTag(MessagingAttributes.CAMUNDA_BPMN_PROCESS_ID, job.BpmnProcessId);
        activity?.AddTag(MessagingAttributes.CAMUNDA_ELEMENT_ID, job.ElementId);
    }

    public static void AddException(this Activity? activity, Exception ex)
    {
        activity?.RecordException(ex);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    }
}
