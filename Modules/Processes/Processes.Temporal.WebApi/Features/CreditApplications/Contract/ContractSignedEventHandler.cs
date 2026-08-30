using Applications.Contracts.Events;
using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Contract;

[WolverineHandler]
public class ContractSignedEventHandler(ITemporalClient messageClient) 
{
    public async Task Handle(ContractSigned message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnContractSignedAsync());
    }
}