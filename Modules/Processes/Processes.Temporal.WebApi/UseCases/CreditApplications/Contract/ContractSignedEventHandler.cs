using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Contract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

public class ContractSignedEventHandler(ITemporalClient messageClient) 
{
    public async Task Handle(ContractSigned message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnContractSignedAsync());
    }
}