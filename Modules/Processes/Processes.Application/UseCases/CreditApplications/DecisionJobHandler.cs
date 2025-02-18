using Camunda.Client.Jobs;
using Processes.Application.Domain.CreditApplications;
using Processes.Application.UseCases.Shared;
using Wolverine;

namespace Processes.Application.UseCases.CreditApplications;

[JobWorker(Type = "credit-decision:1")]
internal class DecisionJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            CustomerVerificationStatus: processInstance.CustomerVerificationStatus,
            SimulationStatus: processInstance.SimulationStatus
        ));
    }
}
