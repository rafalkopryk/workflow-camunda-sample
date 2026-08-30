using Common.Application.Dictionary;
using Wolverine.Attributes;

namespace Applications.Contracts.Commands;

[MessageIdentity("decision", Version = 1)]
public record SetDecisionCommand(string ApplicationId, Decision CustomerVerificationStatus, Decision SimulationStatus);