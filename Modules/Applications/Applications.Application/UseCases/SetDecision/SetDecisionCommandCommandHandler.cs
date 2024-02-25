using Applications.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Common.Kafka;
using MediatR;

namespace Applications.Application.UseCases.SetDecision;

[EventEnvelope(Topic = "command.credit.applications.decision.v1")]
public record SetDecisionCommand(string ApplicationId, Decision Decision) : INotification;

internal class SetDecisionCommandCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IEventBusProducer eventBusProducer,
    TimeProvider timeProvider
    ) : INotificationHandler<SetDecisionCommand>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly IEventBusProducer _eventBusProducer = eventBusProducer;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task Handle(SetDecisionCommand notification, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication.GenerateDecision(notification.Decision, _timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new DecisionGenerated(notification.ApplicationId), cancellationToken);
    }
}

[EventEnvelope(Topic = "event.credit.applications.decisionGenerated.v1")]
public record DecisionGenerated(string ApplicationId) : INotification;
