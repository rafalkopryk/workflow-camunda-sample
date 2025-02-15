//namespace Processes.Saga.WebApi.UseCases.CreditApplications;

//using Wolverine;
//using Wolverine.Persistence.Sagas;

//public class CreditApplicationFast : Saga
//{
//    [SagaIdentity]
//    public string? Id { get; set; }

//    public string Decision { get; set; }

//    public static (CreditApplicationFast, SimulationCommand, CreditApplicationTimeout) Start(ApplicationRegisteredFast application, ILogger<CreditApplicationFast> logger)
//    {
//        var creditApplication = new CreditApplicationFast 
//        { 
//            Id = application.ApplicationId,
//        };

//        var command = new SimulationCommand(application.ApplicationId, application.Amount, application.CreditPeriodInMonths, application.AverageNetMonthlyIncome);

//        return (creditApplication, command, new CreditApplicationTimeout { ApplicationId = application.ApplicationId });
//    }

//    public async Task<DecisionCommand> Consume(SimulationFinishedEvent simulation, ILogger<CreditApplicationFast> logger)
//    {
//        // sample simulation delay
//        await Task.Delay(5000);

//        return new DecisionCommand(simulation.ApplicationId, simulation.Decision);
//    }

//    public async Task Handle(DecisionGenerated decision, ILogger<CreditApplicationFast> logger, IMessageContext messageContext)
//    {
//        Decision = decision.Decision;

//        if (Decision == "Negative")
//        {
//            await messageContext.PublishAsync(new CloseApplicationCommand(Id), new DeliveryOptions
//            {
//                PartitionKey = Id
//            });
//        }
//    }

//    public void Handle(ContractSigned application, ILogger<CreditApplicationFast> logger)
//    {
//        MarkCompleted();
//    }

//    public CloseApplicationCommand Handle(CreditApplicationTimeout application, ILogger<CreditApplicationFast> logger)
//    {
//        return new CloseApplicationCommand(Id);
//    }

//    public void Handle(ApplicationClosed application, ILogger<CreditApplicationFast> logger)
//    {
//        MarkCompleted();
//    }
//}