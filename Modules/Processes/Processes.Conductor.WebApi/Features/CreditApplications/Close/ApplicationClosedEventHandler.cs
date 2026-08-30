using Applications.Contracts.Events;
using Common.Application.Dictionary;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Close;

[WolverineHandler]
public class ApplicationClosedEventHandler(ConductorWorkflowService workflows)
{
    public Task Handle(ApplicationClosed message, CancellationToken cancellationToken)
    {
        return message.Reason == ApplicationCloseReason.CancelledByUser
            ? workflows.TerminateRunningAsync(
                message.ApplicationId,
                ApplicationCloseReason.CancelledByUser.ToString(),
                cancellationToken)
            : workflows.CompleteWaitingTaskAsync(
                message.ApplicationId,
                CreditApplicationConductorNames.WaitApplicationClosedTask,
                cancellationToken: cancellationToken);
    }
}
