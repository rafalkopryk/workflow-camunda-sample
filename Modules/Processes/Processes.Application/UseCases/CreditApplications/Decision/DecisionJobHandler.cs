using Camunda.Client;
using Common.Kafka;
using MediatR;
using Processes.Application.Domain.CreditApplications;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[EventEnvelope(Topic = "command.credit.applications.decision.v1")]
public record DecisionCommand(string ApplicationId, string Decision) : INotification;

[ZeebeWorker(Type = "credit-decision:1", UseStream = true, StreamTimeoutInSec = 60, PoolingDelayInMs = 20_000, PoolingRequestTimeoutInMs = -1)]
internal class DecisionJobHandler(IEventBusProducer busProducer) : IJobHandler
{
    private readonly IEventBusProducer _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.PublishAsync(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Decision: processInstance.Decision
        ),
        cancellationToken);
    }
}
