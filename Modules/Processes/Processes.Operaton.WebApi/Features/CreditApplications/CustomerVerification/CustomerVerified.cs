using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, string CustomerVerificationStatus);
