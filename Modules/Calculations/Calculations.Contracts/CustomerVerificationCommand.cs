using Wolverine.Attributes;

namespace Calculations.Contracts;

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string DocumentId);