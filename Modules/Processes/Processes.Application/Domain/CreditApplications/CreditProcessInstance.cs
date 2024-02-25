namespace Processes.Application.Domain.CreditApplications;

public record CreditProcessInstance
{
    public string ApplicationId { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }

    public string? Decision { get; init; } 
}
