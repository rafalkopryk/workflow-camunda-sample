namespace Calculations.Application.UseCases.VerifyCustomerCommand;

using Wolverine;
using Wolverine.Attributes;

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string Pesel);

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified(string ApplicationId, string Status);

public class CustomerVerificationCommandHandler(IMessageBus eventBusProducer)
{
    public async Task Handle(CustomerVerificationCommand command, CancellationToken cancellationToken)
    {
        var isValid = !string.IsNullOrEmpty(command.Pesel) && command.Pesel.Length == 11;
        var status = isValid ? "Positive" : "Negative";

        await eventBusProducer.PublishAsync(new CustomerVerified(command.ApplicationId, status), new DeliveryOptions
        {
            PartitionKey = command.ApplicationId
        });
    }
}