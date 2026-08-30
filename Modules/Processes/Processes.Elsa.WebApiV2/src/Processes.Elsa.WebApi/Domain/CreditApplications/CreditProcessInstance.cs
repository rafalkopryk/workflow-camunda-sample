namespace Processes.Elsa.WebApi.Domain.CreditApplications;

public sealed record CreditProcessInstance
{
    public required string ApplicationId { get; init; }
    public required string DocumentId { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }
    public TimeSpan Timeout { get; init; }
}

public sealed class CreditApplicationWorkflowOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
}
