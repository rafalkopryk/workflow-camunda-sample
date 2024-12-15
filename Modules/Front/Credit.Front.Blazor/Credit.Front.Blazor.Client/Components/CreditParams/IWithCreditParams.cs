namespace Credit.Front.Blazor.Client.Components.CreditParams;

public interface IWithCreditParams
{
    decimal Amount { get; set; }
    decimal CreditPeriodInMonths { get; set; }
}
