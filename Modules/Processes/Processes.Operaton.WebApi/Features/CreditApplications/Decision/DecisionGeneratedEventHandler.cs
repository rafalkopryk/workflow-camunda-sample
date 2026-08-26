using Processes.Operaton.WebApi.Operaton;
using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Decision;

[WolverineHandler]
public sealed class DecisionGeneratedEventHandler(OperatonClient client)
{
    public Task Handle(DecisionGenerated message, CancellationToken cancellationToken) =>
        client.CorrelateMessageAsync("Message_DecisionGenerated", message.ApplicationId, message, cancellationToken);
}
