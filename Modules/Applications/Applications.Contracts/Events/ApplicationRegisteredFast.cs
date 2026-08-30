using Wolverine.Attributes;

namespace Applications.Contracts.Events;

[MessageIdentity("applicationRegisteredFast", Version = 1)]
public record ApplicationRegisteredFast(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);