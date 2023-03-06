﻿using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Common.Kafka;
using MediatR;

namespace Applications.Application.UseCases.SetDecision;

[EventEnvelope(Topic = "command.credit.applications.decision.start.v1")]
public record SetDecisionCommand(string ApplicationId, Decision Decision) : INotification;

internal class SetDecisionCommandCommandHandler : INotificationHandler<SetDecisionCommand>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;
    private readonly IEventBusProducer _eventBusProducer;

    public SetDecisionCommandCommandHandler(CreditApplicationDbContext creditApplicationDbContext, IEventBusProducer eventBusProducer)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
    }

    public async Task Handle(SetDecisionCommand notification, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(notification.ApplicationId);
        creditApplication.ForwardTo(State.DecisionGenerated(creditApplication.State, notification.Decision, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new SetDecisionCommandDone(notification.ApplicationId), cancellationToken);
    }
}

[EventEnvelope(Topic = "command.credit.applications.decision.done.v1")]
public record SetDecisionCommandDone(string ApplicationId) : INotification;
