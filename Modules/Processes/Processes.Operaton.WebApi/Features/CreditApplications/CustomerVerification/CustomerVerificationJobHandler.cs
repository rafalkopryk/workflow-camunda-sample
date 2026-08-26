using Processes.Operaton.WebApi.Domain;
using Processes.Operaton.WebApi.Operaton;
using Wolverine;

namespace Processes.Operaton.WebApi.Features.CreditApplications.CustomerVerification;

internal sealed class CustomerVerificationJobHandler(IMessageBus bus) : IOperatonJobHandler
{
    public string Topic => "credit-customer-verification:1";

    public async Task HandleAsync(OperatonExternalTask task, CancellationToken cancellationToken)
    {
        var process = task.GetVariables<CreditProcessInstance>();
        await bus.PublishAsync(new CustomerVerificationCommand(process.ApplicationId, process.DocumentId));
    }
}
