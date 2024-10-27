using System.Collections.Immutable;
using Common.Application.Dictionary;

namespace Applications.Application.Domain.Application;

public class CreditApplication
{
    public string Id { get; protected set; }
    public decimal Amount { get; protected set; }
    public int CreditPeriodInMonths { get; protected set; }
    public ApplicationState[] States { get; protected set; } = [];
    public CustomerPersonalData CustomerPersonalData { get; protected set; }
    public Declaration Declaration { get; protected set; }

    public ApplicationState State => States?.OrderByDescending(x => x.Date).FirstOrDefault(); 
    
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
            States =
            [
                new ApplicationState.ApplicationRegistered(timeProvider.GetLocalNow())
            ]
        };
    }

    public void GenerateDecision(Decision decision, TimeProvider timeProvider)
    {
        States = [
            ..States,
            new ApplicationState.DecisionGenerated(timeProvider.GetLocalNow(), decision)
        ];
    }

    public void SignContract(TimeProvider timeProvider)
    {
        States = [
            ..States,
            new ApplicationState.ContractSigned(timeProvider.GetLocalNow())
        ];
    }

    public void CloseApplication(TimeProvider timeProvider)
    {
        States = [
            ..States,
            new ApplicationState.ApplicationClosed(timeProvider.GetLocalNow(), State.Decision)
        ];
    }
}
