using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Contract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);
