using Applications.Application.Features.CloseApplication;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Wolverine;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Close;

[Activity("CreditApplications", "Commands", "Requests closing the credit application.")]
public sealed class PublishCloseApplicationActivity : CodeActivity
{
    [Input]
    public Input<string> ApplicationId { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var bus = context.GetRequiredService<IMessageBus>();
        await bus.PublishAsync(new CloseApplicationCommand(ApplicationId.Get(context)));
    }
}