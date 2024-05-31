using Camunda.Client;
using Common.Application;
using MassTransit;
using MediatR;
using Processes.Application.Domain.CreditApplications;

namespace Processes.Application.UseCases.CreditApplications.Close;

[EntityName("command.credit.applications.close.v1")]
[MessageUrn("command.credit.applications.close.v1")]
public record CloseApplicationCommand(string ApplicationId) : INotification;

[ZeebeWorker(Type = "credit-closeApplication:1", UseStream = true, StreamTimeoutInSec = 120, PoolingDelayInMs = 10_000, PoolingRequestTimeoutInMs = -1)]
internal class CloseApplicationJobHandler(BusProxy busProducer) : IJobHandler
{
    private readonly BusProxy _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.Publish(new CloseApplicationCommand
        (
            ApplicationId: processInstance.ApplicationId
        ),
        cancellationToken);
    }
}
