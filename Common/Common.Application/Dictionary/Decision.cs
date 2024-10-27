using System.Text.Json.Serialization;

namespace Common.Application.Dictionary;

[JsonConverter(typeof(JsonStringEnumConverter<Decision>))]
public enum Decision
{
    NotExists,
    Positive,
    Negative,
}