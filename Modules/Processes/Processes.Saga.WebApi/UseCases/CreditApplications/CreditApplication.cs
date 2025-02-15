namespace Processes.Saga.WebApi.UseCases.CreditApplications;

using Wolverine;
using Wolverine.Persistence.Sagas;

public class CreditApplication : Saga
{
    public const string POSITIVE_KEYWORD = "Postivie";
    public const string NEGATIVE_KEYWORD = "Negative";

    [SagaIdentity]
    public string? Id { get; set; }
    
    //Processs
    public string? SimulationResult { get; set; }
    public string? CustomerVerificationResult { get; set; }

    public bool HasDecision => SimulationResult != null && CustomerVerificationResult != null;
    public string? Decision => HasDecision
        ? SimulationResult == POSITIVE_KEYWORD && CustomerVerificationResult == POSITIVE_KEYWORD
            ? POSITIVE_KEYWORD
            : NEGATIVE_KEYWORD
        : null;

    public static (CreditApplication, SimulationCommand, CustomerVerificationCommand, CreditApplicationTimeout) Start(ApplicationRegistered application, ILogger<CreditApplication> logger)
    {
        var creditApplication = new CreditApplication
        { 
            Id = application.ApplicationId
        };

        var simulationCommand = new SimulationCommand(application.ApplicationId, application.Amount, application.CreditPeriodInMonths, application.AverageNetMonthlyIncome);
        var customerVerificationCommand = new CustomerVerificationCommand(application.ApplicationId, application.Pesel);

        return (creditApplication, simulationCommand, customerVerificationCommand, new CreditApplicationTimeout { ApplicationId = application.ApplicationId });
    }

    public async Task Handle(CustomerVerified verification, ILogger<CreditApplication> logger, IMessageContext messageContext)
    {
        CustomerVerificationResult = verification.Status;

        if (HasDecision)
        {
            await messageContext.PublishAsync(new DecisionCommand(Id, Decision), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public async Task Consume(SimulationFinishedEvent simulation, ILogger<CreditApplication> logger, IMessageContext messageContext)
    {
        SimulationResult = simulation.Decision;

        if (HasDecision)
        {
            await messageContext.PublishAsync(new DecisionCommand(Id, Decision), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public async Task Handle(DecisionGenerated decision, ILogger<CreditApplication> logger, IMessageContext messageContext)
    {
        if (decision.Decision == NEGATIVE_KEYWORD)
        {
            await messageContext.PublishAsync(new CloseApplicationCommand(Id), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public void Handle(ContractSigned application, ILogger<CreditApplication> logger)
    {
        MarkCompleted();
    }

    public CloseApplicationCommand Handle(CreditApplicationTimeout application, ILogger<CreditApplication> logger)
    {
        return new CloseApplicationCommand(Id);
    }

    public void Handle(ApplicationClosed application, ILogger<CreditApplication> logger)
    {
        MarkCompleted();
    }
}

