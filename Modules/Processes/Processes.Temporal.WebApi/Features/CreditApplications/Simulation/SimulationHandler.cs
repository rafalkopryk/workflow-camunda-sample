using Calculations.Contracts;
using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Activities;
using Wolverine;

namespace Processes.Temporal.WebApi.Features.CreditApplications.Simulation;

internal class SimulationService(IMessageBus busProducer) 
{
    [Activity("Simulate")]
    public async Task StartSimulation(CreditProcessInstance processInstance)
    {
        await busProducer.PublishAsync(new SimulationCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Amount: processInstance.Amount,
            AverageNetMonthlyIncome: processInstance.AverageNetMonthlyIncome,
            CreditPeriodInMonths: processInstance.CreditPeriodInMonths
        ));
    }
}

