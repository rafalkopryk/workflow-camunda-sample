using Camunda.Client;
using Common.Kafka;
using MediatR;
using Processes.Application.Domain.CreditApplications;

namespace Processes.Application.UseCases.CreditApplications.Simulation;

[EventEnvelope(Topic = "command.credit.calculations.simulation.v1")]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome) : INotification;

[ZeebeWorker(Type = "credit-simulation:1", UseStream = true, StreamTimeoutInSec = 60, PoolingDelayInMs = 20_000, PoolingRequestTimeoutInMs = -1)]
internal class SimulationJobHandler(IEventBusProducer busProducer) : IJobHandler
{
    private readonly IEventBusProducer _busProducer = busProducer;

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var processInstance = job.GetVariablesAsType<CreditProcessInstance>();
        await _busProducer.PublishAsync(new SimulationCommand
        (
            ApplicationId: processInstance.ApplicationId,
            Amount: processInstance.Amount,
            AverageNetMonthlyIncome: processInstance.AverageNetMonthlyIncome,
            CreditPeriodInMonths: processInstance.CreditPeriodInMonths
        ),
        cancellationToken);
    }
}
