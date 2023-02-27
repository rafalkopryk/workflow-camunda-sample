using Calculations.Application.Domain;
using Calculations.Application.Infrastructure.Database;
using Camunda.Client;
using Common.Application.Dictionary;
using Common.Application.Serializer;
using System.Text.Json;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

[ZeebeWorker(Type = "simulate-credit", MaxJobsToActivate = 10, PollingTimeoutInMs = 15_000, PollIntervalInMs = 500, RetryBackOffInMs = new[] { 1_000, 5_000 }, AutoComplate = false)]
internal class SimulateCreditCommandHandler : IJobHandler
{
    private readonly CreditCalculationDbContext _creditCalculationDbContext;

    public SimulateCreditCommandHandler(CreditCalculationDbContext creditCalculationDbContext)
    {
        _creditCalculationDbContext = creditCalculationDbContext;
    }

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var creditApplication = JsonSerializer.Deserialize<CreditApplication>(job.Variables, Camunda.Client.JsonSerializerCustomOptions.CamelCase);
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

        await _creditCalculationDbContext.SaveChangesAsync(cancellationToken);

        await client.CompleteJobCommand(job, JsonSerializer.Serialize(new { calculation.Decision, creditApplication.ApplicationId }, Camunda.Client.JsonSerializerCustomOptions.CamelCase));
    }

    private record CreditApplication(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);
}
