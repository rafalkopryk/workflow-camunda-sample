using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Registration;

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(
    string ApplicationId,
    decimal Amount,
    int CreditPeriodInMonths,
    decimal AverageNetMonthlyIncome,
    string DocumentId);
