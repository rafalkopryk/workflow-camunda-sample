using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[MessageIdentity("decision", Version=1)]
public record DecisionCommand(string ApplicationId, string CustomerVerificationStatus, string SimulationStatus);

internal class DecisionJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var processInstance = job.GetVariables<CreditProcessInstance>();
        await busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            CustomerVerificationStatus: processInstance.CustomerVerificationStatus,
            SimulationStatus: processInstance.SimulationStatus
        ));
    }
}
