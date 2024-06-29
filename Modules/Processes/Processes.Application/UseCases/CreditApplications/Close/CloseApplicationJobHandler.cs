using Camunda.Client;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Close;

[MessageIdentity("close", Version=1)]
public record CloseApplicationCommand(string ApplicationId);

[ZeebeWorker(Type = "credit-closeApplication:1", UseStream = true, StreamTimeoutInSec = 120, PoolingDelayInMs = 10_000, PoolingRequestTimeoutInMs = -1)]
internal class CloseApplicationJobHandler(IMessageBus busProducer) : IJobHandler
{
    private readonly IMessageBus _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.PublishAsync(new CloseApplicationCommand
        (
            ApplicationId: processInstance.ApplicationId
        ));
    }
}
