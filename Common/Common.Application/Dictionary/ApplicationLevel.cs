using System.Text.Json.Serialization;

namespace Common.Application.Dictionary;

[JsonConverter(typeof(JsonStringEnumConverter<ApplicationLevel>))]
public enum ApplicationLevel
{
    ApplicationRegistered,
    DecisionGenerated,
    ApplicationClosed,
    ContractSigned,
}