using Common.Application;
using Common.Application.Dictionary;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Applications.Application.Domain.Application;

public class CreditApplication
{
    public string ApplicationId { get; protected set; }
    public decimal Amount { get; protected set; }
    public int CreditPeriodInMonths { get; protected set; }
    public List<State> States { get; protected set; } = new();
    public CustomerPersonalData CustomerPersonalData { get; protected set; }
    public Declaration Declaration { get; protected set; }
    public State State => States?.OrderByDescending(x => x.Date).FirstOrDefault(); 
    
    protected CreditApplication() { }

    public static CreditApplication Create(
        string applicationId,
        decimal amount,
        int creditPeriodInMonths,
        CustomerPersonalData customerPersonalData,
        Declaration declaration)
    {
        return new CreditApplication
        {
            ApplicationId = applicationId,
            Amount = amount,
            CreditPeriodInMonths = creditPeriodInMonths,
            CustomerPersonalData = customerPersonalData,
            Declaration = declaration,
            States = new List<State>
            {
                State.ApplicationRegistered(DateTimeProvider.Shared.GetLocalNow()),
            }
        };
    }

    public void GenerateDecision(Decision decision)
    {
        States.Add(State.DecisionGenerated(State, decision, DateTimeProvider.Shared.GetLocalNow()));
    }

    public void SignContract()
    {
        States.Add(State.ContractSigned(State, DateTimeProvider.Shared.GetLocalNow()));
    }

    public void CloseApplication()
    {
        States.Add(State.ApplicationClosed(State, DateTimeProvider.Shared.GetLocalNow()));
    }
}
