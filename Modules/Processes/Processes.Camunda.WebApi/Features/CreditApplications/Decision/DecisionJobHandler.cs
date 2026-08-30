using Applications.Contracts.Commands;
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk;
using Processes.Camunda.WebApi.Domain.CreditApplications;
using Wolverine;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Decision;

internal class DecisionJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var processInstance = job.GetVariables<CreditProcessInstance>();
        await busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            CustomerVerificationStatus: processInstance.CustomerVerificationStatus ?? Common.Application.Dictionary.Decision.NotExists,
            SimulationStatus: processInstance.SimulationStatus ?? Common.Application.Dictionary.Decision.NotExists
        ));
    }
}
