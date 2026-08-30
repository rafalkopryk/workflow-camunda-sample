using Applications.Contracts.Commands;
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk;
using Processes.Camunda.WebApi.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Close;

[WolverineHandler]
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
