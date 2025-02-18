using Camunda.Client.Jobs;
using Processes.Application.Domain.FastCreditApplications;
using Processes.Application.UseCases.Shared;
using Wolverine;

namespace Processes.Application.UseCases.FastCreditApplications;

[JobWorker(Type = "fast_credit-simulation:1")]
internal class SimulationJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await busProducer.PublishAsync(new SimulationCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Amount: processInstance.Amount,
            AverageNetMonthlyIncome: processInstance.AverageNetMonthlyIncome,
            CreditPeriodInMonths: processInstance.CreditPeriodInMonths
        ));
    }
}
