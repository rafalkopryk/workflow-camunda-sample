using Applications.Application.Infrastructure.Database;
using Applications.Contracts.Events;
using Common.Application.Dictionary;
using Wolverine;

namespace Applications.Application.Features.CloseApplication;

public class CloseApplicationCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus eventBusProducer,
    TimeProvider timeProvider)
{
    public async Task Handle(CloseApplicationCommand notification)
    {
        var creditApplication = await creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication!.CloseApplication(timeProvider);

        await creditApplicationDbContext.SaveChangesAsync();

        await eventBusProducer.PublishAsync(new ApplicationClosed(
            notification.ApplicationId,
            ApplicationCloseReason.NegativeDecision), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });
    }
}