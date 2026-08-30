using Wolverine.Attributes;

namespace Calculations.Contracts;

[MessageIdentity("simulation", Version = 1)]
public record SimulateCreditCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);