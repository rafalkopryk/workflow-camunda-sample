using Conductor.Api;
using Conductor.Client.Models;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Shared;

public sealed class ConductorWorkflowService(
    WorkflowResourceApi workflowApi,
    TaskResourceApi taskApi)
{
    public async System.Threading.Tasks.Task CompleteWaitingTaskAsync(
        string applicationId,
        string taskReferenceName,
        Dictionary<string, object>? output = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflow = (await workflowApi.GetWorkflowsAsync(
                CreditApplicationConductorNames.WorkflowName,
                applicationId,
                includeClosed: false,
                includeTasks: false))
            .Single();

        await taskApi.UpdateTaskSyncAsync(
            output ?? [],
            workflow.WorkflowId,
            taskReferenceName,
            TaskResult.StatusEnum.COMPLETED);
    }

    public async System.Threading.Tasks.Task TerminateRunningAsync(
        string applicationId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflows = await workflowApi.GetWorkflowsAsync(
            CreditApplicationConductorNames.WorkflowName,
            applicationId,
            includeClosed: true,
            includeTasks: false);

        var runningWorkflowIds = workflows
            .Where(workflow => workflow.Status == Workflow.StatusEnum.RUNNING)
            .Select(workflow => workflow.WorkflowId)
            .ToList();

        await System.Threading.Tasks.Task.WhenAll(runningWorkflowIds.Select(workflowId =>
            workflowApi.TerminateAsync(workflowId, reason)));
    }
}
