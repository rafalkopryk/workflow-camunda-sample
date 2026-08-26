using Processes.Operaton.WebApi.Domain;
using Processes.Operaton.WebApi.Operaton;
using Wolverine;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Close;

internal sealed class CloseApplicationJobHandler(IMessageBus bus) : IOperatonJobHandler
{
    public string Topic => "credit-closeApplication:1";

    public async Task HandleAsync(OperatonExternalTask task, CancellationToken cancellationToken)
    {
        var process = task.GetVariables<CreditProcessInstance>();
        await bus.PublishAsync(new CloseApplicationCommand(process.ApplicationId));
    }
}
