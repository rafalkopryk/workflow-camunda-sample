namespace Calculations.Application.UseCases.SimulateCreditCommand;

using Calculations.Application.Domain;
using Calculations.Application.Infrastructure.Database;
using Common.Application;
using Common.Application.Dictionary;

using MassTransit;
using MediatR;

[EntityName("command.credit.calculations.simulation.v1")]
[MessageUrn("command.credit.calculations.simulation.v1")]
public record SimulateCreditCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

[EntityName("event.credit.calculations.simulationFinished.v1")]
[MessageUrn("event.credit.calculations.simulationFinished.v1")]
public record SimulationCreditFinished(string ApplicationId, string Decision);

internal class SimulateCreditCommandHandler(CreditCalculationDbContext creditCalculationDbContext, BusProxy eventBusProducer) : IConsumer<SimulateCreditCommand>
{
    private readonly CreditCalculationDbContext _creditCalculationDbContext = creditCalculationDbContext;

    private readonly BusProxy _eventBusProducer = eventBusProducer;

    public async Task Consume(ConsumeContext<SimulateCreditCommand> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(SimulateCreditCommand notification, CancellationToken cancellationToken)
    {
        var decsion = notification switch
        {
            { Amount: < 1000 or > 25000 } => Decision.Negative,
            { CreditPeriodInMonths: < 6 or > 24 } => Decision.Negative,
            { AverageNetMonthlyIncome: < 2000 } => Decision.Negative,
            _ => Decision.Positive
        };

        var calculation = new CreditCalculation
        {
            ApplicationId = notification.ApplicationId,
            Amount = notification.Amount,
            CreditPeriodInMonths = notification.CreditPeriodInMonths,
            Decision = decsion,
        };

        await _creditCalculationDbContext.Calculations.AddAsync(calculation, cancellationToken);

        await _creditCalculationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.Publish(new SimulationCreditFinished(calculation.ApplicationId, calculation.Decision.ToString()), cancellationToken);
    }
}
