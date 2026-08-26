using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.CustomerVerification;

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string DocumentId);
