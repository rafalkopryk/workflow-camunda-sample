using Wolverine.Attributes;

namespace Applications.Contracts.Events;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);