using System.Text.Json.Serialization;

namespace CreditProcessFunctions.UseCases.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<DecisionEnum>))]
public enum DecisionEnum
{
    Positive,
    Negative,
}