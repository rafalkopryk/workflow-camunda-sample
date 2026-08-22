using Common.Application.Dictionary;

namespace Applications.Application.Domain.Application;

public class CreditApplication
{
    public string Id { get; protected init; }
    public decimal Amount { get; protected init; }
    public int CreditPeriodInMonths { get; protected init; }
    public List<ApplicationState> States { get; protected set; }
    public CustomerPersonalData CustomerPersonalData { get; protected init; }
    public Declaration Declaration { get; protected init; }

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
                ApplicationState.ApplicationRegistered(timeProvider.GetLocalNow())
            ],
        };
    }

    public void GenerateDecision(Decision decision, TimeProvider timeProvider)
    {
        States = States.Append(ApplicationState.DecisionGenerated(timeProvider.GetLocalNow(), decision));
    }

    public void SignContract(TimeProvider timeProvider)
    {
        States = States.Append(ApplicationState.ContractSigned(timeProvider.GetLocalNow()));
    }

    public void CloseApplication(TimeProvider timeProvider)
    {
        States = States.Append(ApplicationState.ApplicationClosed(timeProvider.GetLocalNow(), States.Current.Decision));
    }
}
