using Applications.Contracts.Commands;
using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Activities;
using Wolverine;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Decision;

internal class DecisionService(IMessageBus busProducer)
{
    [Activity("credit-decision:1")]
    public async Task Handle(CreditProcessInstance processInstance)
    {
        await busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            CustomerVerificationStatus: processInstance.CustomerVerificationStatus ?? Common.Application.Dictionary.Decision.NotExists,
            SimulationStatus: processInstance.SimulationStatus ?? Common.Application.Dictionary.Decision.NotExists
        ));
    }
}
