using Applications.Application.Infrastructure.Database;
using Wolverine;
using Wolverine.Attributes;

namespace Applications.Application.Features.CloseApplication;

[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);

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

        await eventBusProducer.PublishAsync(new ApplicationClosed(notification.ApplicationId), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });
    }
}

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);
