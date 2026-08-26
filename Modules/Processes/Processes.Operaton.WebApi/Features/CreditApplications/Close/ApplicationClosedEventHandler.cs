using Processes.Operaton.WebApi.Operaton;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Close;

public sealed class ApplicationClosedEventHandler(OperatonClient client)
{
    public Task Handle(ApplicationClosed message, CancellationToken cancellationToken) =>
        client.CorrelateMessageAsync(
            "Message_ApplicationClosed",
            message.ApplicationId,
            message,
            cancellationToken,
            correlateAll: true);
}
