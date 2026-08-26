using Processes.Operaton.WebApi.Domain;
using Processes.Operaton.WebApi.Operaton;
using Wolverine;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Simulation;

internal sealed class SimulationJobHandler(IMessageBus bus) : IOperatonJobHandler
{
    public string Topic => "credit-simulation:1";

    public async Task HandleAsync(OperatonExternalTask task, CancellationToken cancellationToken)
    {
        var process = task.GetVariables<CreditProcessInstance>();
        await bus.PublishAsync(new SimulationCommand(
            process.ApplicationId,
            process.Amount,
            process.CreditPeriodInMonths,
            process.AverageNetMonthlyIncome));
    }
}
