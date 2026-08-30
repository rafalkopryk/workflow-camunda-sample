using Calculations.Contracts;
using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Activities;
using Wolverine;

namespace Processes.Temporal.WebApi.Features.CreditApplications.CustomerVerification;

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