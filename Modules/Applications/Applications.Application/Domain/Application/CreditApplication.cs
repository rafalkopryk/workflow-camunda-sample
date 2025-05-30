﻿using System.Collections.Immutable;
using Common.Application.Dictionary;

namespace Applications.Application.Domain.Application;

public class CreditApplication
{
    public string Id { get; protected set; }
    public decimal Amount { get; protected set; }
    public int CreditPeriodInMonths { get; protected set; }
    public ApplicationStates States { get; protected set; }
    public CustomerPersonalData CustomerPersonalData { get; protected set; }
    public Declaration Declaration { get; protected set; }

    protected CreditApplication() { }

    public static CreditApplication Create(
        string applicationId,
        decimal amount,
        int creditPeriodInMonths,
        CustomerPersonalData customerPersonalData,
        Declaration declaration,
        TimeProvider timeProvider)
    {
        return new CreditApplication
        {
            Id = applicationId,
            Amount = amount,
            CreditPeriodInMonths = creditPeriodInMonths,
            CustomerPersonalData = customerPersonalData,
            Declaration = declaration,
            States = new ApplicationStates(
            [
                new ApplicationState.ApplicationRegistered(timeProvider.GetLocalNow())
            ]),
        };
    }

    public void GenerateDecision(Decision decision, TimeProvider timeProvider)
    {
        States = States.Append(new ApplicationState.DecisionGenerated(timeProvider.GetLocalNow(), decision));
    }

    public void SignContract(TimeProvider timeProvider)
    {
        States = States.Append(new ApplicationState.ContractSigned(timeProvider.GetLocalNow()));
    }

    public void CloseApplication(TimeProvider timeProvider)
    {
        States = States.Append(new ApplicationState.ApplicationClosed(timeProvider.GetLocalNow(), States.Current.Decision));
    }
}
