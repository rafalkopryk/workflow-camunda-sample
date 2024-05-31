using Camunda.Client;
using Common.Application;
using Common.Kafka;
using MassTransit;
using MediatR;
using Processes.Application.Domain.CreditApplications;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[EntityName("command.credit.calculations.simulation.v1")]
[MessageUrn("command.credit.calculations.simulation.v1")]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome) : INotification;

[ZeebeWorker(Type = "credit-simulation:1", UseStream = true, StreamTimeoutInSec = 60, PoolingDelayInMs = 20_000, PoolingRequestTimeoutInMs = -1)]
internal class SimulationJobHandler(BusProxy busProducer) : IJobHandler
{
    private readonly BusProxy _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.Publish(new SimulationCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Amount: processInstance.Amount,
            AverageNetMonthlyIncome: processInstance.AverageNetMonthlyIncome,
            CreditPeriodInMonths: processInstance.CreditPeriodInMonths
        ),
        cancellationToken);
    }
}
