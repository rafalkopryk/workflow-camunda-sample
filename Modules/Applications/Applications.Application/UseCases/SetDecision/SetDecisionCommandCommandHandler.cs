using Applications.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Wolverine;
using Wolverine.Attributes;

namespace Applications.Application.UseCases.SetDecision;

[MessageIdentity("decision", Version = 1)]
public record SetDecisionCommand(string ApplicationId, Decision Decision);

public class SetDecisionCommandCommandHandler
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;
    private readonly IMessageBus _eventBusProducer;
    private readonly TimeProvider _timeProvider;

    public SetDecisionCommandCommandHandler(CreditApplicationDbContext creditApplicationDbContext, IMessageBus eventBusProducer, TimeProvider timeProvider)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
        _timeProvider = timeProvider;
    }

    public async Task Handle(SetDecisionCommand notification)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);
        creditApplication.GenerateDecision(notification.Decision, _timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync();

        await _eventBusProducer.PublishAsync(new DecisionGenerated(notification.ApplicationId, notification.Decision), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });
    }
}

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, Decision Decision);
