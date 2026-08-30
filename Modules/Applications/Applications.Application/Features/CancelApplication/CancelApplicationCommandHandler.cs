using Applications.Application.Features.CloseApplication;
using Applications.Application.Infrastructure.Database;
using Applications.Contracts.Events;
using Common.Application.Cqrs;
using Common.Application.Dictionary;
using Wolverine;
using static Applications.Application.Features.CancelApplication.CancelApplicationCommandResponse;

namespace Applications.Application.Features.CancelApplication;

internal class CancelApplicationCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus eventBusProducer,
    TimeProvider timeProvider
    ) : IRequestHandler<CancelApplicationCommand, CancelApplicationCommandResponse>
{
    public async Task<CancelApplicationCommandResponse> Handle(CancelApplicationCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
        {
            return ResourceNotFound.Result;
        }

        creditApplication.CloseApplication(timeProvider);

        await creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await eventBusProducer.PublishAsync(new ApplicationClosed(
            creditApplication.Id,
            ApplicationCloseReason.CancelledByUser), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });

        return OK.Result;
    }
}
