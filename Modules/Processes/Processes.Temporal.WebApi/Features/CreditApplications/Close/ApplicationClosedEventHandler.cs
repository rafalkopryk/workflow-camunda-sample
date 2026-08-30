using Applications.Contracts.Events;
using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Close;

[WolverineHandler]
public class ApplicationClosedEventHandler(ITemporalClient messageClient) 
{
    public async Task Handle(ApplicationClosed message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnCancelledAsync());
    }
}
