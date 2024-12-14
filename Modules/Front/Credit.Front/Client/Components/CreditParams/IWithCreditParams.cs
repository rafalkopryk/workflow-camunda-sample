namespace Credit.Front.Client.Components.CreditParams;

public interface IWithCreditParams
{
    decimal Amount { get; set; }
    decimal CreditPeriodInMonths { get; set; }
}
