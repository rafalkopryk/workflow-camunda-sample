namespace Calculations.Application.UseCases.VerifyCustomerCommand;

using Common.Application.Dictionary;
using Wolverine;
using Wolverine.Attributes;

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string Pesel);

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, Decision CustomerVerificationStatus);

public class CustomerVerificationCommandHandler(IMessageBus eventBusProducer)
{
    public async Task Handle(CustomerVerificationCommand command, CancellationToken cancellationToken)
    {
        var isValid = !string.IsNullOrEmpty(command.Pesel) && command.Pesel.Length == 11;
        var status = isValid ? Decision.Positive : Decision.Negative;

        await eventBusProducer.PublishAsync(new CustomerVerified(command.ApplicationId, status), new DeliveryOptions
        {
            PartitionKey = command.ApplicationId
        });
    }
}