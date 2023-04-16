using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Kafka;
using MediatR;

namespace Applications.Application.UseCases.CloseApplication;

[EventEnvelope(Topic = "command.credit.applications.close.v1")]
public record CloseApplicationCommand(string ApplicationId) : INotification;

internal class CloseApplicationCommandHandler : INotificationHandler<CloseApplicationCommand>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    private readonly IEventBusProducer _eventBusProducer;

    public CloseApplicationCommandHandler(CreditApplicationDbContext creditApplicationDbContext, IEventBusProducer eventBusProducer)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
    }

    public async Task Handle(CloseApplicationCommand notification, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication.ForwardTo(State.ApplicationClosed(creditApplication.State, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new ApplicationClosed(notification.ApplicationId), cancellationToken);
    }
}

[EventEnvelope(Topic = "event.credit.applications.applicationClosed.v1")]
public record ApplicationClosed(string ApplicationId) : INotification;
