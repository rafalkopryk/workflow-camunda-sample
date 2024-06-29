using Applications.Application.Infrastructure.Database;
using Common.Application;
using Common.Application.Dictionary;
using MassTransit;

namespace Applications.Application.UseCases.SetDecision;

[EntityName("command.credit.applications.decision.v1")]
[MessageUrn("command.credit.applications.decision.v1")]
public record SetDecisionCommand(string ApplicationId, Decision Decision);

public class SetDecisionCommandCommandHandler : IConsumer<SetDecisionCommand>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;
    private readonly BusProxy _eventBusProducer;
    private readonly TimeProvider _timeProvider;

    public SetDecisionCommandCommandHandler(CreditApplicationDbContext creditApplicationDbContext, BusProxy eventBusProducer, TimeProvider timeProvider)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<SetDecisionCommand> context)
    {
        await Handle(context.Message, context.CancellationToken);
    }

    public async Task Handle(SetDecisionCommand notification, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication.GenerateDecision(notification.Decision, _timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.Publish(new DecisionGenerated(notification.ApplicationId, notification.Decision), cancellationToken);
    }
}

[EntityName("event.credit.applications.decisionGenerated.v1")]
[MessageUrn("event.credit.applications.decisionGenerated.v1")]
public record DecisionGenerated(string ApplicationId, Decision Decision);
