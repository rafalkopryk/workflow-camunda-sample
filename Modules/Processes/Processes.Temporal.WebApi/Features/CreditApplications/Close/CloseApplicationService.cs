using Applications.Contracts.Commands;
using Temporalio.Activities;
using Wolverine;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Close;

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
