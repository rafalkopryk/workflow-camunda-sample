using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Client.Models;
using Conductor.Client.Worker;
using Processes.Conductor.WebApi.Domain.CreditApplications;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Runtime;
using ConductorTask = Conductor.Client.Models.Task;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Simulation;

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(
    string ApplicationId,
    decimal Amount,
    int CreditPeriodInMonths,
    decimal AverageNetMonthlyIncome);

internal sealed class SimulationTaskWorker(IWolverineRuntime runtime) : IWorkflowTask
{
    private readonly IMessageBus _bus = new MessageBus(runtime);

    public string TaskType => CreditApplicationConductorNames.SimulationTask;

    public WorkflowTaskExecutorConfiguration WorkerSettings { get; } = new();

    public async System.Threading.Tasks.Task<TaskResult> Execute(
        ConductorTask task,
        CancellationToken token = default)
    {
        var processInstance = task.InputData.GetVariables<CreditProcessInstance>();

        await _bus.PublishAsync(new SimulationCommand(
            processInstance.ApplicationId,
            processInstance.Amount,
            processInstance.CreditPeriodInMonths,
            processInstance.AverageNetMonthlyIncome));

        return task.Completed();
    }

    [Obsolete("Required by conductor-csharp 3.0.0; asynchronous execution is used by the worker host.")]
    public TaskResult Execute(ConductorTask task) => Execute(task, CancellationToken.None).GetAwaiter().GetResult();
}
