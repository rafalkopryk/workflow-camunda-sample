using Calculations.Contracts;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Processes.Elsa.WebApi.Domain.CreditApplications;
using Wolverine;

namespace Processes.Elsa.WebApi.Features.CreditApplications.CustomerVerification;

[Activity("CreditApplications", "Commands", "Requests customer verification.")]
public sealed class PublishCustomerVerificationActivity : CodeActivity
{
    [Input]
    public Input<CreditProcessInstance> ProcessInstance { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var process = ProcessInstance.Get(context);
        var bus = context.GetRequiredService<IMessageBus>();

        await bus.PublishAsync(new CustomerVerificationCommand(process.ApplicationId, process.DocumentId));
    }
}