namespace Credit.Front.Blazor.Client.Components.CustomerDeclarations;

public interface IWithDeclerations
{
    DeclarationDto Declaration { get; init; }
}

public record DeclarationDto
{
    public decimal AverageNetMonthlyIncome { get; set; }
}
