using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Activities;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string DocumentId);

internal class CustomerVerificationService(IMessageBus busProducer)
{
    [Activity("credit-customer-verification:1")]
    public async Task StartCustomerVerification(CreditProcessInstance processInstance)
    {
        await busProducer.PublishAsync(new CustomerVerificationCommand(
            ApplicationId: processInstance.ApplicationId,
            DocumentId: processInstance.DocumentId
        ));
    }
}