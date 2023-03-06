namespace Calculations.Application.UseCases.SimulateCreditCommand;

using Calculations.Application.Domain;
using Calculations.Application.Infrastructure.Database;
using Common.Application.Dictionary;

using Common.Kafka;
using MediatR;

[EventEnvelope(Topic = "command.credit.calculations.simulation.start.v1")]
public record SimulateCreditCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome) : INotification;

[EventEnvelope(Topic = "command.credit.calculations.simulation.done.v1")]
public record SimulateCreditCommandDone(string ApplicationId, string Decision) : INotification;

internal class SimulateCreditCommandHandler : INotificationHandler<SimulateCreditCommand>
{
    private readonly CreditCalculationDbContext _creditCalculationDbContext;

    private readonly IEventBusProducer _eventBusProducer;

    public SimulateCreditCommandHandler(CreditCalculationDbContext creditCalculationDbContext, IEventBusProducer eventBusProducer)
    {
        _creditCalculationDbContext = creditCalculationDbContext;
        _eventBusProducer = eventBusProducer;
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

        await _eventBusProducer.PublishAsync(new SimulateCreditCommandDone(calculation.ApplicationId, calculation.Decision.ToString()), cancellationToken);
    }
}
