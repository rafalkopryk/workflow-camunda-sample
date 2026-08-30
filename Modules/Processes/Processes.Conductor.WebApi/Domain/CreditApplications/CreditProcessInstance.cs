using Common.Application.Dictionary;

namespace Processes.Conductor.WebApi.Domain.CreditApplications;

public sealed record CreditProcessInstance
{
    public required string ApplicationId { get; init; }
    public string? DocumentId { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }
    public Decision? CustomerVerificationStatus { get; init; }
    public Decision? SimulationStatus { get; init; }
    public Decision? Decision { get; init; }
}
