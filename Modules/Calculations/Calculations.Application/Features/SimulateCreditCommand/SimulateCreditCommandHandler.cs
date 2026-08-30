using Calculations.Application.Domain;
using Calculations.Application.Infrastructure.Database;
using Calculations.Contracts;
using Common.Application.Dictionary;
using Wolverine;

namespace Calculations.Application.Features.SimulateCreditCommand;

public class SimulateCreditCommandHandler(CreditCalculationDbContext creditCalculationDbContext, IMessageBus eventBusProducer)
{
    public async Task Handle(Contracts.SimulateCreditCommand notification, CancellationToken cancellationToken)
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

        await creditCalculationDbContext.Calculations.AddAsync(calculation, cancellationToken);

        await creditCalculationDbContext.SaveChangesAsync(cancellationToken);

        await eventBusProducer.PublishAsync(new SimulationCreditFinished(calculation.ApplicationId, calculation.Decision.ToString()), new DeliveryOptions
        {
            PartitionKey = calculation.ApplicationId
        });
    }
}
