using Wolverine.Attributes;

namespace Processes.Application.UseCases.Shared;

[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string Pesel);

[MessageIdentity("decision", Version = 1)]
public record DecisionCommand(string ApplicationId, string CustomerVerificationStatus, string SimulationStatus);

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);