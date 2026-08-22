using Applications.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Wolverine;
using Wolverine.Attributes;

namespace Applications.Application.Features.SetDecision;

[MessageIdentity("decision", Version = 1)]
public record SetDecisionCommand(string ApplicationId, Decision CustomerVerificationStatus, Decision SimulationStatus);

public class SetDecisionCommandCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus eventBusProducer,
    TimeProvider timeProvider)
{
    public async Task Handle(SetDecisionCommand notification)
    {
        var creditApplication = await creditApplicationDbContext.GetCreditApplicationAsync(notification.ApplicationId);

        var decision = (notification.CustomerVerificationStatus, notification.SimulationStatus) switch
        {
            (Decision.Positive, Decision.Positive) => Decision.Positive,
            _ => Decision.Negative,
        };

        creditApplication!.GenerateDecision(decision, timeProvider);

        await creditApplicationDbContext.SaveChangesAsync();

        await eventBusProducer.PublishAsync(new DecisionGenerated(notification.ApplicationId, decision), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });
    }
}

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated(string ApplicationId, Decision Decision);
