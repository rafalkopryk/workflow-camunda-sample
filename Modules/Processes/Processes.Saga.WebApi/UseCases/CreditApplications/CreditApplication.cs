namespace Processes.Saga.WebApi.UseCases.CreditApplications;

using Wolverine;
using Wolverine.Persistence.Sagas;

public class CreditApplication : Saga
{
    public const string POSITIVE_KEYWORD = "Positive";
    public const string NEGATIVE_KEYWORD = "Negative";

    [SagaIdentity]
    public string? Id { get; set; }
    
    //Processs
    public string? SimulationStatus { get; set; }
    public string? CustomerVerificationStatus { get; set; }

    public bool HasDecision => SimulationStatus != null && CustomerVerificationStatus != null;
    public string? Decision { get; set; }

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
        CustomerVerificationStatus = verification.CustomerVerificationStatus;

        if (HasDecision)
        {
            await messageContext.PublishAsync(new DecisionCommand(Id, SimulationStatus, CustomerVerificationStatus), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public async Task Handle(SimulationFinishedEvent simulation, ILogger<CreditApplication> logger, IMessageContext messageContext)
    {
        SimulationStatus = simulation.SimulationStatus;

        if (HasDecision)
        {
            await messageContext.PublishAsync(new DecisionCommand(Id, SimulationStatus, CustomerVerificationStatus), new DeliveryOptions
            {
                PartitionKey = Id
            });
        }
    }

    public async Task Handle(DecisionGenerated decision, ILogger<CreditApplication> logger, IMessageContext messageContext)
    {
        Decision = decision.Decision;

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

