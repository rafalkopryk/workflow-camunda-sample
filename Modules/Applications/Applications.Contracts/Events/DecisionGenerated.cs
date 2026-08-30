using Common.Application.Dictionary;
using Wolverine.Attributes;

namespace Applications.Contracts.Events;

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, Decision Decision);