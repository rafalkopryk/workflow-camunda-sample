namespace Calculations.Application.UseCases.SimulateCreditCommand;

using Calculations.Application.Domain;
using Calculations.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Wolverine;
using Wolverine.Attributes;

[MessageIdentity("simulation", Version = 1)]
public record SimulateCreditCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationCreditFinished(string ApplicationId, string Decision);

public class SimulateCreditCommandHandler(CreditCalculationDbContext creditCalculationDbContext, IMessageBus eventBusProducer)
{
    private readonly CreditCalculationDbContext _creditCalculationDbContext = creditCalculationDbContext;

    private readonly IMessageBus _eventBusProducer = eventBusProducer;

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
            Id = Guid.CreateVersion7(),
            ApplicationId = notification.ApplicationId,
            Amount = notification.Amount,
            CreditPeriodInMonths = notification.CreditPeriodInMonths,
            Decision = decsion,
        };

        await _creditCalculationDbContext.Calculations.AddAsync(calculation, cancellationToken);

        await _creditCalculationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new SimulationCreditFinished(calculation.ApplicationId, calculation.Decision.ToString()), new DeliveryOptions
        {
            PartitionKey = calculation.ApplicationId
        });
    }
}
