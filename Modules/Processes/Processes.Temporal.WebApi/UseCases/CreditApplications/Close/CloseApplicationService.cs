using Temporalio.Activities;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Close;

[MessageIdentity("close", Version=1)]
public record CloseApplicationCommand(string ApplicationId);

internal class CloseApplicationService(IMessageBus busProducer)
{
    [Activity("credit-closeApplication:1")]
    public async Task Handle(string applicationId)
    {
        await busProducer.PublishAsync(new CloseApplicationCommand
        (
            ApplicationId: applicationId
        ));
    }
}
