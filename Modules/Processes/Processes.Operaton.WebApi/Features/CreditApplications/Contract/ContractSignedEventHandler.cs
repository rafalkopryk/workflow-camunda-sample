using Processes.Operaton.WebApi.Operaton;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Contract;

public sealed class ContractSignedEventHandler(OperatonClient client)
{
    public Task Handle(ContractSigned message, CancellationToken cancellationToken) =>
        client.CorrelateMessageAsync("Message_ContractSigned", message.ApplicationId, message, cancellationToken);
}
