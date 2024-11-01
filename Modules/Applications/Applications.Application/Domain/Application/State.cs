using System.Text.Json.Serialization;
using Common.Application.Dictionary;
using MongoDB.Bson.Serialization.Attributes;

namespace Applications.Application.Domain.Application;

[BsonKnownTypes(typeof(ApplicationRegistered), typeof(DecisionGenerated), typeof(ApplicationClosed), typeof(ContractSigned))]
[JsonDerivedType(typeof(ApplicationRegistered), nameof(ApplicationRegistered))]
[JsonDerivedType(typeof(DecisionGenerated), nameof(DecisionGenerated))]
[JsonDerivedType(typeof(ApplicationClosed), nameof(ApplicationClosed))]
[JsonDerivedType(typeof(ContractSigned), nameof(ContractSigned))]
public abstract record ApplicationState(DateTimeOffset Date, Decision Decision)
{
    public string Level => GetType().Name;
    
    public record ApplicationRegistered(DateTimeOffset Date) : ApplicationState(Date, Decision.NotExists);

    public record DecisionGenerated(DateTimeOffset Date, Decision Decision) : ApplicationState(Date, Decision);

    public record ApplicationClosed(DateTimeOffset Date, Decision Decision) : ApplicationState(Date, Decision);

    public record ContractSigned(DateTimeOffset Date) : ApplicationState(Date, Decision.Positive);
}

public record ApplicationStates(ApplicationState[] History)
{
    public ApplicationState? Current => History.OrderByDescending(x => x.Date).FirstOrDefault();

    public ApplicationStates Append(ApplicationState state)
    {
        return new ApplicationStates([..History, state]);
    }
}