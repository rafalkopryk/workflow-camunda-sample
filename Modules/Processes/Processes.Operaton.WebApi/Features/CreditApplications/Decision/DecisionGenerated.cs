using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Decision;

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, string Decision);
