using Camunda.Client;
using Common.Application;
using MassTransit;
using MediatR;
using Processes.Application.Domain.CreditApplications;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[EntityName("command.credit.applications.decision.v1")]
[MessageUrn("command.credit.applications.decision.v1")]
public record DecisionCommand(string ApplicationId, string Decision) : INotification;

[ZeebeWorker(Type = "credit-decision:1", UseStream = true, StreamTimeoutInSec = 120, PoolingDelayInMs = 10_000, PoolingRequestTimeoutInMs = -1)]
internal class DecisionJobHandler(BusProxy busProducer) : IJobHandler
{
    private readonly BusProxy _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.Publish(new DecisionCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Decision: processInstance.Decision
        ),
        cancellationToken);
    }
}
