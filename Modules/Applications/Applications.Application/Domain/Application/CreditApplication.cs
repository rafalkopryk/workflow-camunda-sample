namespace Applications.Application.Domain.Application;

public class CreditApplication
{
    public Guid ApplicationId { get; protected set; }
    public decimal Amount { get; protected set; }
    public int CreditPeriodInMonths { get; protected set; }
    public List<State> States { get; protected set; }
    public CustomerPersonalData CustomerPersonalData { get; protected set; }
    public Declaration Declaration { get; protected set; }
    public State State => States?.OrderByDescending(x => x.Date).FirstOrDefault(); 
    
    protected CreditApplication() { }

    public static CreditApplication Create(
        Guid applicationId,
        decimal amount,
        int creditPeriodInMonths,
        CustomerPersonalData customerPersonalData,
        Declaration declaration,
        DateTimeOffset date)
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
                State.ApplicationRegistered(date),
            }
        };
    }

    public void ForwardTo(State state)
    {
        States.Add(state);
    }
}
