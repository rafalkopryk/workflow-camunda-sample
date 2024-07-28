using Camunda.Client;
using Processes.Application.Domain.CreditApplications;
using Wolverine;
using Wolverine.Attributes;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

[JobWorker(Type = "credit-simulation:1", UseStream = true, StreamTimeoutInSec = 60, PoolingDelayInMs = 20_000, PoolingRequestTimeoutInMs = -1)]
internal class SimulationJobHandler(IMessageBus busProducer) : IJobHandler
{
    private readonly IMessageBus _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.PublishAsync(new SimulationCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Amount: processInstance.Amount,
            AverageNetMonthlyIncome: processInstance.AverageNetMonthlyIncome,
            CreditPeriodInMonths: processInstance.CreditPeriodInMonths
        ));
    }
}
