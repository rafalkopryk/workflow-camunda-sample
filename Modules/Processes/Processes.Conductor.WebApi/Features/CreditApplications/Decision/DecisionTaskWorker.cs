using Applications.Contracts.Commands;
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Processes.Conductor.WebApi.Domain.CreditApplications;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine;
using Wolverine.Runtime;
using ConductorTask = Conductor.Client.Models.Task;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Decision;

internal sealed class DecisionTaskWorker(IWolverineRuntime runtime) : IWorkflowTask
{
    private readonly IMessageBus _bus = new MessageBus(runtime);

    public string TaskType => CreditApplicationConductorNames.DecisionTask;

    public WorkflowTaskExecutorConfiguration WorkerSettings { get; } = new()
    {
        PollInterval = TimeSpan.FromMilliseconds(500),
    };

    public async System.Threading.Tasks.Task<TaskResult> Execute(
        ConductorTask task,
        CancellationToken token = default)
    {
        var processInstance = task.InputData.GetVariables<CreditProcessInstance>();

        await _bus.PublishAsync(new DecisionCommand(
            processInstance.ApplicationId,
            processInstance.CustomerVerificationStatus ?? Common.Application.Dictionary.Decision.NotExists,
            processInstance.SimulationStatus ?? Common.Application.Dictionary.Decision.NotExists));

        return task.Completed();
    }

    [Obsolete("Required by conductor-csharp 3.0.0; asynchronous execution is used by the worker host.")]
    public TaskResult Execute(ConductorTask task) => Execute(task, CancellationToken.None).GetAwaiter().GetResult();
}
