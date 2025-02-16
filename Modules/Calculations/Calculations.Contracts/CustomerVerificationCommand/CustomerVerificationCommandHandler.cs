namespace Calculations.Contracts.UseCases.VerifyCustomerCommand;


[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string Pesel);

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, string CustomerVerificationStatus);