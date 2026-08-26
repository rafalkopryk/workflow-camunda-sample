using Processes.Operaton.WebApi.Operaton;

namespace Processes.Operaton.WebApi.Features.CreditApplications.CustomerVerification;

public sealed class CustomerVerificationFinishedHandler(OperatonClient client)
{
    public Task Handle(CustomerVerified message, CancellationToken cancellationToken) =>
        client.CorrelateMessageAsync("Message_CustomerVerified", message.ApplicationId, message, cancellationToken);
}
