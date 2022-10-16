namespace Applications.Application.UseCases.RegisterApplication.Dto;

public record RegisterApplicationCommandDeclarationDto
{
    public decimal AverageNetMonthlyIncome { get; init; }
}