using Calculations.Application.Domain;
using Calculations.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Common.Application.Serializer;
using Common.Application.Zeebe;
using MediatR;
using System.Text.Json;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

internal class SimulateCreditCommandHandler : IRequestHandler<SimulateCreditCommand>
{
    private readonly IZeebeService _zeebeService;
    private readonly CreditCalculationDbContext _creditCalculationDbContext;

    public SimulateCreditCommandHandler(IZeebeService zeebeService, CreditCalculationDbContext creditCalculationDbContext)
    {
        _zeebeService = zeebeService;
        _creditCalculationDbContext = creditCalculationDbContext;
    }

    public async Task<Unit> Handle(SimulateCreditCommand request, CancellationToken cancellationToken)
    {
        var creditApplication = JsonSerializer.Deserialize<CreditApplication>(request.Job.Variables, JsonSerializerCustomOptions.CamelCase);

        var decsion = creditApplication switch
        {
            { Amount: < 1000 or > 25000} => Decision.Negative,
            { CreditPeriodInMonths: < 6 or > 24 } => Decision.Negative,
            { AverageNetMonthlyIncome: < 2000 } => Decision.Negative,
            _ => Decision.Positive
        };

        var calculation = new CreditCalculation
        {
            ApplicationId = creditApplication.ApplicationId,
            Amount = creditApplication.Amount,
            CreditPeriodInMonths = creditApplication.CreditPeriodInMonths,
            Decision = decsion,
        };

        await _creditCalculationDbContext.Calculations.AddAsync(calculation, cancellationToken);

        await _zeebeService.SetVeriables(request.Job, new { calculation.Decision, creditApplication.ApplicationId }, cancellationToken);

        await _creditCalculationDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private record CreditApplication
    {
        public Guid ApplicationId { get; init; }

        public decimal Amount { get; init; }

        public int CreditPeriodInMonths { get; init; }

        public decimal AverageNetMonthlyIncome { get; init; }
    }
}
