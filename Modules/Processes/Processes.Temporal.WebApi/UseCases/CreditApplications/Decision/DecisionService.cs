using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Activities;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Decision;

[MessageIdentity("decision", Version=1)]
public record DecisionCommand(string ApplicationId, string CustomerVerificationStatus, string SimulationStatus);

internal class DecisionService(IMessageBus busProducer)
{
    [Activity("credit-decision:1")]
    public async Task Handle(CreditProcessInstance processInstance)
    {
        await busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            CustomerVerificationStatus: processInstance.CustomerVerificationStatus,
            SimulationStatus: processInstance.SimulationStatus
        ));
    }
}
