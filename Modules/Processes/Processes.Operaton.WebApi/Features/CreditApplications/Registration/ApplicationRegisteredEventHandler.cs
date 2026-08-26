using Processes.Operaton.WebApi.Operaton;
using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Registration;

[WolverineHandler]
public sealed class ApplicationRegisteredEventHandler(OperatonClient client)
{
    public Task Handle(ApplicationRegistered message, CancellationToken cancellationToken) =>
        client.CorrelateMessageAsync(
            "Message_ApplicationRegistered",
            message.ApplicationId,
            message,
            cancellationToken);
}
