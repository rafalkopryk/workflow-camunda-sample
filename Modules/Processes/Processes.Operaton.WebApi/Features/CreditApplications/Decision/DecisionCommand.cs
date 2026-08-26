using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Decision;

[MessageIdentity("decision", Version = 1)]
public record DecisionCommand(
    string ApplicationId,
    string CustomerVerificationStatus,
    string SimulationStatus);
