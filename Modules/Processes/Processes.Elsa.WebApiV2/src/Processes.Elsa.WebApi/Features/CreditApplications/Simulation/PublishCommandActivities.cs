using Calculations.Contracts;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Processes.Elsa.WebApi.Domain.CreditApplications;
using Wolverine;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Simulation;

[Activity("CreditApplications", "Commands", "Requests a credit simulation.")]
public sealed class PublishSimulationActivity : CodeActivity
{
    [Input]
    public Input<CreditProcessInstance> ProcessInstance { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var process = ProcessInstance.Get(context);
        var bus = context.GetRequiredService<IMessageBus>();

        await bus.PublishAsync(new SimulateCreditCommand(
            process.ApplicationId,
            process.Amount,
            process.CreditPeriodInMonths,
            process.AverageNetMonthlyIncome));
    }
}