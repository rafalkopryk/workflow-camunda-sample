using Common.Application.Dictionary;
using Wolverine.Attributes;

namespace Calculations.Contracts;

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, Decision CustomerVerificationStatus);