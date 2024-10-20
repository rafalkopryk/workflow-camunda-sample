using Common.Application.Dictionary;
namespace Calculations.Application.Domain;

public class CreditCalculation
{
    public Guid Id { get; set; }

    public string ApplicationId { get; set; }

    public decimal Amount { get; set; }

    public int CreditPeriodInMonths { get; set; }

    public Decision Decision { get; set; }
}
