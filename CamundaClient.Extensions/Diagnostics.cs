using System.Diagnostics;
using Camunda.Orchestration.Sdk;

namespace Camunda.Client.Extensions;

internal static class Diagnostics
{
    internal const string ActivitySourceName = "Camunda.Client.Extensions";
    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    internal static Activity? StartActivity(ActivatedJob job)
    {
        var activity = ActivitySource.StartActivity(
            $"{job.Type} process",
            ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "camunda");
        activity?.SetTag("messaging.destination", job.Type);
        activity?.SetTag("messaging.operation", "process");
        activity?.SetTag("camunda.job_key", job.JobKey);
        activity?.SetTag("camunda.element_id", job.ElementId);
        activity?.SetTag("camunda.process_definition_id", job.ProcessDefinitionId);
        activity?.SetTag("camunda.process_instance_key", job.ProcessInstanceKey);

        return activity;
    }
}
