using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Close;

[MessageIdentity("close", Version=1)]
public record CloseApplicationCommand(string ApplicationId);

internal class CloseApplicationJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var processInstance = job.GetVariables<CreditProcessInstance>();
        await busProducer.PublishAsync(new CloseApplicationCommand
        (
            ApplicationId: processInstance.ApplicationId
        ));
    }
}
