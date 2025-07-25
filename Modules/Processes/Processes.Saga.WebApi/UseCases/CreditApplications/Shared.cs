﻿namespace Processes.Saga.WebApi.UseCases.CreditApplications;

using JasperFx.Core;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Persistence.Sagas;


[MessageIdentity("applicationTimeouted", Version = 1)]
public record CreditApplicationTimeout() : TimeoutMessage(5.Minutes())
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
};

[MessageIdentity("applicationRegisteredFast", Version = 1)]
public record ApplicationRegisteredFast 
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }
};

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public string DocumentId { get; init; }
    public decimal Amount { get; init; }
    public int CreditPeriodInMonths { get; init; }
    public decimal AverageNetMonthlyIncome { get; init; }
};

[MessageIdentity("decisionGenerated", Version = 1)]
public record DecisionGenerated
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public string Decision { get; init; }
}


[MessageIdentity("simulationFinished", Version = 1)]
public record SimulationFinishedEvent
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public string SimulationStatus { get; init; }
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

[MessageIdentity("customerVerified", Version = 1)]
public record CustomerVerified
{
    [SagaIdentity]
    public string ApplicationId { get; init; }
    public string CustomerVerificationStatus { get; init; }
}

[MessageIdentity("simulation", Version = 1)]
public record SimulationCommand(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);

[MessageIdentity("decision", Version = 1)]
public record DecisionCommand(string ApplicationId, string SimulationStatus, string CustomerVerificationStatus);

[MessageIdentity("customerVerification", Version = 1)]
public record CustomerVerificationCommand(string ApplicationId, string DocumentId);


