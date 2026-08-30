namespace Applications.Application.Features.RegisterApplication.Dto;

public record ApplicationDto
{
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
}