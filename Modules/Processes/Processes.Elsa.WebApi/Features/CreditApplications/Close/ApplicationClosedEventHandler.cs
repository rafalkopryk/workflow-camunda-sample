using Applications.Contracts.Events;
using Common.Application.Dictionary;
using Elsa.Workflows.Runtime;
using Wolverine.Attributes;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Close;

[WolverineHandler]
public class ApplicationClosedEventHandler(IEventPublisher publisher)
{
    public async Task Handle(ApplicationClosed message)
    {
        await publisher.PublishAsync(
            message.Reason == ApplicationCloseReason.CancelledByUser
                ? CreditApplicationEventNames.ApplicationCancelled
                : CreditApplicationEventNames.ApplicationClosed,
            message.ApplicationId,
            payload: message);
    }
}
