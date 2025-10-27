using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Activities;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Simulation;

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

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

