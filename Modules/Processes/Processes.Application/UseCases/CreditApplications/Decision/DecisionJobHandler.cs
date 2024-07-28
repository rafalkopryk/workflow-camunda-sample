using Camunda.Client;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[MessageIdentity("decision", Version=1)]
public record DecisionCommand(string ApplicationId, string Decision);

[JobWorker(Type = "credit-decision:1", UseStream = true, StreamTimeoutInSec = 120, PoolingDelayInMs = 10_000, PoolingRequestTimeoutInMs = -1)]
internal class DecisionJobHandler(IMessageBus busProducer) : IJobHandler
{
    private readonly IMessageBus _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Decision: processInstance.Decision
        ));
    }
}
