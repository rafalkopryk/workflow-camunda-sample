using Common.Application.Dictionary;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Applications.Application.Domain.Application;

public class CreditApplication
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; protected set; }
    public decimal Amount { get; protected set; }
    public int CreditPeriodInMonths { get; protected set; }
    public List<State> States { get; protected set; } = [];
    public CustomerPersonalData CustomerPersonalData { get; protected set; }
    public Declaration Declaration { get; protected set; }

    public State State => States?.OrderByDescending(x => x.Date).FirstOrDefault(); 
    
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
                State.ApplicationRegistered(timeProvider.GetLocalNow()),
            ]
        };
    }

    public void GenerateDecision(Decision decision, TimeProvider timeProvider)
    {
        States.Add(State.DecisionGenerated(State, decision, timeProvider.GetLocalNow()));
    }

    public void SignContract(TimeProvider timeProvider)
    {
        States.Add(State.ContractSigned(State, timeProvider.GetLocalNow()));
    }

    public void CloseApplication(TimeProvider timeProvider)
    {
        States.Add(State.ApplicationClosed(State, timeProvider.GetLocalNow()));
    }
}
