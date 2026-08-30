using Common.Application.Dictionary;
using Wolverine.Attributes;

namespace Calculations.Contracts;

[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationFinished(string ApplicationId, Decision SimulationStatus);