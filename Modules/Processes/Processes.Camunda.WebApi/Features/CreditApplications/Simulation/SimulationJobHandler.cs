using Calculations.Contracts;
using Camunda.Client.Extensions;
using Camunda.Orchestration.Sdk;
using Processes.Camunda.WebApi.Domain.CreditApplications;
using Wolverine;

namespace Processes.Camunda.WebApi.Features.CreditApplications.Simulation;

internal class SimulationJobHandler(IMessageBus busProducer) : IJobHandler
{
    public async Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var processInstance = job.GetVariables<CreditProcessInstance>();
        await busProducer.PublishAsync(new SimulationCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Amount: processInstance.Amount,
            AverageNetMonthlyIncome: processInstance.AverageNetMonthlyIncome,
            CreditPeriodInMonths: processInstance.CreditPeriodInMonths
        ));
    }
}
