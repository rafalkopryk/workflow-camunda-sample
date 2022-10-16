namespace Applications.Application.UseCases.GetApplication.Dto;

public record GetApplicationQueryDeclarationDto
{
    public decimal AverageNetMonthlyIncome { get; init; }
}