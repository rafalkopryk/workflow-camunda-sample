using Calculations.Contracts;
using Common.Application.Dictionary;
using Wolverine;

namespace Calculations.Application.Features.CustomerVerificationCommand;

public class CustomerVerificationCommandHandler(IMessageBus eventBusProducer)
{
    public async Task Handle(Contracts.CustomerVerificationCommand command, CancellationToken cancellationToken)
    {
        var isValid = !string.IsNullOrEmpty(command.DocumentId) && command.DocumentId.Length > 5;
        var status = isValid ? Decision.Positive : Decision.Negative;

        await eventBusProducer.PublishAsync(new CustomerVerified(command.ApplicationId, status), new DeliveryOptions
        {
            PartitionKey = command.ApplicationId
        });
    }
}