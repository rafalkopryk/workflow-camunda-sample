using Camunda.Client.Jobs;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string DocumentId);

[JobWorker(Type = "credit-customer-verification:1")]
internal class CustomerVerificationJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        
        await busProducer.PublishAsync(new CustomerVerificationCommand(
            ApplicationId: processInstance.ApplicationId,
            DocumentId: processInstance.DocumentId
        ));
    }
}