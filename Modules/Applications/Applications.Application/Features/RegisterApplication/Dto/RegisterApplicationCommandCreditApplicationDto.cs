namespace Applications.Application.Features.RegisterApplication.Dto;

public record RegisterApplicationCommandCreditApplicationDto
{
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }

    public RegisterApplicationCommandCustomerPersonalDto CustomerPersonalData { get; init; }

    public RegisterApplicationCommandDeclarationDto Declaration { get; init; }
}
