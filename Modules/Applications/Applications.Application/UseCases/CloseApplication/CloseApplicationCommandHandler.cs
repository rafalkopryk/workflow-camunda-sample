using Applications.Application.Infrastructure.Database;
using Common.Application;
using MassTransit;

namespace Applications.Application.UseCases.CloseApplication;

[EntityName("command.credit.applications.close.v1")]
[MessageUrn("command.credit.applications.close.v1")]
public record CloseApplicationCommand(string ApplicationId);

internal class CloseApplicationCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    BusProxy eventBusProducer,
    TimeProvider timeProvider
    ): IConsumer<CloseApplicationCommand>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;

    private readonly BusProxy _eventBusProducer = eventBusProducer;

    private readonly TimeProvider timeProvider = timeProvider;

    public async Task Consume(ConsumeContext<CloseApplicationCommand> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(CloseApplicationCommand notification, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication.CloseApplication(timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.Publish(new ApplicationClosed(notification.ApplicationId), cancellationToken);
    }
}

[EntityName("event.credit.applications.applicationClosed.v1")]
[MessageUrn("event.credit.applications.applicationClosed.v1")]
public record ApplicationClosed(string ApplicationId);
