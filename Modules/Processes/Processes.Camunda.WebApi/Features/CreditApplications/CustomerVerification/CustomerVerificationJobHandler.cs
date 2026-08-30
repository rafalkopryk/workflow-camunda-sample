using Calculations.Contracts;
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk;
using Processes.Camunda.WebApi.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Camunda.WebApi.Features.CreditApplications.CustomerVerification;

[WolverineHandler]
internal class CustomerVerificationJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var processInstance = job.GetVariables<CreditProcessInstance>();

        await busProducer.PublishAsync(new CustomerVerificationCommand(
            ApplicationId: processInstance.ApplicationId,
            DocumentId: processInstance.DocumentId
        ));
    }
}
