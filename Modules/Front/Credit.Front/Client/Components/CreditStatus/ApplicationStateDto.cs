namespace Credit.Front.Client.Components.CreditStatus;

public interface IWithCreditApplicationState
{
    ApplicationStateDto State { get; init; }
}

public record ApplicationStateDto
{
    public Level Level { get; set; }
    public DateTimeOffset Date { get; set; }
    public DateTimeOffset? ContractSigningDate { get; set; }
    public Decision Decision { get; set; }
};

public enum Level
{
    ApplicationRegistered, DecisionGenerated, ContractSigned, ApplicationClosed
}

public enum Decision
{
    NotExists, Positive, Negative
}