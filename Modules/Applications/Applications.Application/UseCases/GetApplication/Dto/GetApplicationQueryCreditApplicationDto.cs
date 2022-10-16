namespace Applications.Application.UseCases.GetApplication.Dto;

public record GetApplicationQueryCreditApplicationDto
{
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public GetApplicationQueryCustomerPersonalDataDto CustomerPersonalData { get; init; }
    public GetApplicationQueryDeclarationDto Declaration { get; init; }
    public GetApplicationStateDto State { get; init; }
}
