using Wolverine;
using Wolverine.Persistence.Sagas;
using Wolverine.Configuration;
using Wolverine.ErrorHandling;
using Wolverine.Runtime.Handlers;
using Applications.Contracts.Commands;
using Applications.Contracts.Events;
using Calculations.Contracts;
using Common.Application.Dictionary;

namespace Processes.Saga.WebApi.Features.CreditApplications;

public class CreditApplication : Wolverine.Saga, IHandlerConfiguration
{
    [SagaIdentity]
    public string? Id { get; set; }
    
    public Decision? SimulationStatus { get; set; }
    public Decision? CustomerVerificationStatus { get; set; }

    public bool HasDecision => SimulationStatus != null && CustomerVerificationStatus != null;
    public Decision? Decision { get; set; }
    public bool DecisionRequested { get; set; }

    public static void Configure(HandlerChain chain)
    {
        chain.OnException<SagaConcurrencyException>()
            .RetryWithCooldown(
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromSeconds(1));
    }

    public static (CreditApplication, SimulationCommand, CustomerVerificationCommand, CreditApplicationTimeout) Start(
        [SagaIdentityFrom(nameof(ApplicationRegistered.ApplicationId))] ApplicationRegistered application,
        ILogger<CreditApplication> logger)
    {
        var creditApplication = new CreditApplication
        { 
            Id = application.ApplicationId
        };

        var simulationCommand = new SimulationCommand(application.ApplicationId, application.Amount, application.CreditPeriodInMonths, application.AverageNetMonthlyIncome);
        var customerVerificationCommand = new CustomerVerificationCommand(application.ApplicationId, application.DocumentId);

        return (creditApplication, simulationCommand, customerVerificationCommand, new CreditApplicationTimeout { ApplicationId = application.ApplicationId });
    }

    public async Task Handle(
        [SagaIdentityFrom(nameof(CustomerVerified.ApplicationId))] CustomerVerified verification,
        ILogger<CreditApplication> logger,
        IMessageContext messageContext)
    {
        CustomerVerificationStatus = verification.CustomerVerificationStatus;

        if (HasDecision && !DecisionRequested)
        {
            DecisionRequested = true;
            await messageContext.PublishAsync(new DecisionCommand(Id!, CustomerVerificationStatus!.Value, SimulationStatus!.Value), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public async Task Handle(
        [SagaIdentityFrom(nameof(SimulationFinished.ApplicationId))] SimulationFinished simulation,
        ILogger<CreditApplication> logger,
        IMessageContext messageContext)
    {
        SimulationStatus = simulation.SimulationStatus;

        if (HasDecision && !DecisionRequested)
        {
            DecisionRequested = true;
            await messageContext.PublishAsync(new DecisionCommand(Id!, CustomerVerificationStatus!.Value, SimulationStatus!.Value), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public async Task Handle(
        [SagaIdentityFrom(nameof(DecisionGenerated.ApplicationId))] DecisionGenerated decision,
        ILogger<CreditApplication> logger,
        IMessageContext messageContext)
    {
        Decision = decision.Decision;

        if (decision.Decision == global::Common.Application.Dictionary.Decision.Negative)
        {
            await messageContext.PublishAsync(new CloseApplicationCommand(Id!), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public void Handle(
        [SagaIdentityFrom(nameof(ContractSigned.ApplicationId))] ContractSigned application,
        ILogger<CreditApplication> logger)
    {
        MarkCompleted();
    }

    public CloseApplicationCommand Handle(
        [SagaIdentityFrom(nameof(CreditApplicationTimeout.ApplicationId))] CreditApplicationTimeout application,
        ILogger<CreditApplication> logger)
    {
        return new CloseApplicationCommand(Id!);
    }

    public void Handle(
        [SagaIdentityFrom(nameof(ApplicationClosed.ApplicationId))] ApplicationClosed application,
        ILogger<CreditApplication> logger)
    {
        MarkCompleted();
    }
}
