using Common.Application.Dictionary;
using Wolverine.Attributes;

namespace Applications.Contracts.Commands;

[MessageIdentity("decision", Version = 1)]
public record DecisionCommand(string ApplicationId, Decision CustomerVerificationStatus, Decision SimulationStatus);