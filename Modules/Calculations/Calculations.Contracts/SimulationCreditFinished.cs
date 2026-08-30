using Wolverine.Attributes;

namespace Calculations.Contracts;

[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationCreditFinished(string ApplicationId, string SimulationStatus);