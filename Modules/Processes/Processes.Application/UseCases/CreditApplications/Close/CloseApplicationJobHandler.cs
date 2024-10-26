using Camunda.Client.Jobs;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Close;

[MessageIdentity("close", Version=1)]
public record CloseApplicationCommand(string ApplicationId);

[JobWorker(Type = "credit-closeApplication:1")]
internal class CloseApplicationJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await busProducer.PublishAsync(new CloseApplicationCommand
        (
            ApplicationId: processInstance.ApplicationId
        ));
    }
}
