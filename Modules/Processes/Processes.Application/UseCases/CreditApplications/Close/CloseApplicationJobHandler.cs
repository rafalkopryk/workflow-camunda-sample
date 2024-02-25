using Camunda.Client;
using Common.Kafka;
using MediatR;
using Processes.Application.Domain.CreditApplications;

namespace Processes.Application.UseCases.CreditApplications.Close;

[EventEnvelope(Topic = "command.credit.applications.close.v1")]
public record CloseApplicationCommand(string ApplicationId) : INotification;

[ZeebeWorker(Type = "credit-closeApplication:1", UseStream = true, StreamTimeoutInSec = 60, PoolingDelayInMs = 20_000, PoolingRequestTimeoutInMs = -1)]
internal class CloseApplicationJobHandler(IEventBusProducer busProducer) : IJobHandler
{
    private readonly IEventBusProducer _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.PublishAsync(new CloseApplicationCommand
        (
            ApplicationId: processInstance.ApplicationId
        ),
        cancellationToken);
    }
}
