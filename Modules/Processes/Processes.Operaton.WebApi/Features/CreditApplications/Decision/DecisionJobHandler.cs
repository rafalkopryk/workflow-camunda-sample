using Processes.Operaton.WebApi.Domain;
using Processes.Operaton.WebApi.Operaton;
using Wolverine;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Decision;

internal sealed class DecisionJobHandler(IMessageBus bus) : IOperatonJobHandler
{
    public string Topic => "credit-decision:1";

    public async Task HandleAsync(OperatonExternalTask task, CancellationToken cancellationToken)
    {
        var process = task.GetVariables<CreditProcessInstance>();
        await bus.PublishAsync(new DecisionCommand(
            process.ApplicationId,
            process.CustomerVerificationStatus!,
            process.SimulationStatus!));
    }
}
