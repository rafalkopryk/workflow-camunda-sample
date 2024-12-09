namespace Credit.Front.Client.Components;

public interface IWithCreditParams
{
    decimal Amount { get; init; }
    decimal CreditPeriodInMonths { get; init; }
}

public interface IWithDeclerations
{
    DeclarationDto Declaration { get; init; }
}

public interface IWithCustomerPersonal
{
    CustomerPersonalDto CustomerPersonalData { get; init; }
}

public interface IWithCreditApplicationState
{
    ApplicationState State { get; init; }
}

public record CustomerPersonalDto(string FirstName, string LastName, string Pesel);

public record DeclarationDto(decimal AverageNetMonthlyIncome);


public record ApplicationState(Level Level, DateTimeOffset Date, DateTimeOffset? ContractSigningDate, Decision Decision);

public enum Level
{
    ApplicationRegistered, DecisionGenerated, ContractSigned, ApplicationClosed
}

public enum Decision
{
    NotExists, Positive, Negative
}