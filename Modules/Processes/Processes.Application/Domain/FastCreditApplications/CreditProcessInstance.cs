namespace Processes.Application.Domain.FastCreditApplications;

public record CreditProcessInstance
{
    public required string ApplicationId { get; init; }
    public required string Pesel { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }
    public string? SimulationStatus { get; init; }
    public string? Decision { get; init; }
}
