using Applications.Contracts.Events;
using Elsa.Workflows.Runtime;
using Wolverine.Attributes;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Contract;

[WolverineHandler]
public class ContractSignedEventHandler(IEventPublisher publisher)
{
    public async Task Handle(ContractSigned message)
    {
        await publisher.PublishAsync(
            CreditApplicationEventNames.ContractSigned,
            message.ApplicationId,
            payload: message);
    }
}