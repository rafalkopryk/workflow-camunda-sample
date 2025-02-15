using Applications.Application.Infrastructure.Database;
using Wolverine;
using Wolverine.Attributes;

namespace Applications.Application.UseCases.CloseApplication;


[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);

public class CloseApplicationCommandHandler
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    private readonly IMessageBus _eventBusProducer;

    private readonly TimeProvider _timeProvider;

    public CloseApplicationCommandHandler(CreditApplicationDbContext creditApplicationDbContext, IMessageBus eventBusProducer, TimeProvider timeProvider)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
        _timeProvider = timeProvider;
    }

    public async Task Handle(CloseApplicationCommand notification)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication.CloseApplication(_timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync();

        await _eventBusProducer.PublishAsync(new ApplicationClosed(notification.ApplicationId), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });
    }
}

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);
