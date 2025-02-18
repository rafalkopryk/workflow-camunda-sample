using Camunda.Client.Jobs;
using Processes.Application.Domain.FastCreditApplications;
using Processes.Application.UseCases.Shared;
using Wolverine;

namespace Processes.Application.UseCases.FastCreditApplications;

[JobWorker(Type = "fast_credit-decision:1")]
internal class DecisionJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            CustomerVerificationStatus: "Positive", //only for sample
            SimulationStatus: processInstance.SimulationStatus
        ));
    }
}
