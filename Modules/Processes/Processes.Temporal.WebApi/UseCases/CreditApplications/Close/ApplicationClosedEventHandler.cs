using Temporalio.Client;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Close;

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);

public class ApplicationClosedEventHandler( ITemporalClient messageClient) 
{
    public async Task Handle(ApplicationClosed message)
    {
        var handler = messageClient.GetWorkflowHandle<CreditApplicationWorkflow>(message.ApplicationId);
        await handler.SignalAsync(x => x.OnCancelledAsync());
    }
}
