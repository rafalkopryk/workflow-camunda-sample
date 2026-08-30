using Applications.Application.Infrastructure.Database;
using Applications.Contracts.Commands;
using Applications.Contracts.Events;
using Common.Application.Dictionary;
using Wolverine;

namespace Applications.Application.Features.SetDecision;

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