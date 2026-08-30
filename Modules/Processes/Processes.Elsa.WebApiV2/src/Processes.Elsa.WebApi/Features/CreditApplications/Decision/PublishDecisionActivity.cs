using Applications.Contracts.Commands;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Wolverine;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Decision;

[Activity("CreditApplications", "Commands", "Requests the credit decision.")]
public sealed class PublishDecisionActivity : CodeActivity
{
    [Input]
    public Input<string> ApplicationId { get; set; }

    [Input]
    public Input<Common.Application.Dictionary.Decision> CustomerVerificationStatus { get; set; }

    [Input]
    public Input<Common.Application.Dictionary.Decision> SimulationStatus { get; set; } 

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var bus = context.GetRequiredService<IMessageBus>();
        await bus.PublishAsync(new DecisionCommand(
            ApplicationId.Get(context),
            CustomerVerificationStatus.Get(context),
            SimulationStatus.Get(context)));
    }
}