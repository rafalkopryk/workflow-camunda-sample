using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Simulation;

[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationFinished(string ApplicationId, string SimulationStatus);
