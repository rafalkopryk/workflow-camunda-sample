using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Simulation;

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(
    string ApplicationId,
    decimal Amount,
    int CreditPeriodInMonths,
    decimal AverageNetMonthlyIncome);
