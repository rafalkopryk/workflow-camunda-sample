namespace Processes.Saga.WebApi.UseCases.CreditApplications;

using JasperFx.Core;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Persistence.Sagas;

public class CreditApplication : Saga
{
    [SagaIdentity]
    public string? Id { get; set; }

    public string Decision { get; set; }

    public static (CreditApplication, SimulationCommand, CreditApplicationTimeout) Start(ApplicationRegistered application, ILogger<CreditApplication> logger)
    {
        var creditApplication = new CreditApplication 
        { 
            Id = application.ApplicationId,
        };

        var command = new SimulationCommand(application.ApplicationId, application.Amount, application.CreditPeriodInMonths, application.AverageNetMonthlyIncome);

        return (creditApplication, command, new CreditApplicationTimeout { ApplicationId = application.ApplicationId });
    }

    public async Task<DecisionCommand> Handle(SimulationFinished simulation, ILogger<CreditApplication> logger)
    {
        // sample simulation delay
        await Task.Delay(5000);

        return new DecisionCommand(simulation.ApplicationId, simulation.Decision);
    }

    public async Task Handle(DecisionGenerated decision, ILogger<CreditApplication> logger, IMessageContext messageContext)
    {
        Decision = decision.Decision;

        if (Decision == "Negative")
        {
            await messageContext.PublishAsync(new CloseApplicationCommand(Id));
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

[MessageIdentity("applicationTimeouted", Version = 1)]
public record CreditApplicationTimeout() : TimeoutMessage(5.Minutes())
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
};

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered 
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }
};

[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationFinished
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public string Decision { get; init; }
}

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public string Decision { get; init; }
}


[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
}

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
}

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);


[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);


[MessageIdentity("decision", Version = 1)]
public record DecisionCommand(string ApplicationId, string Decision);

