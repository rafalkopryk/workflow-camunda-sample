using System.Text.Json.Serialization;

namespace Common.Application.Dictionary;

[JsonConverter(typeof(JsonStringEnumConverter<Decision>))]
public enum Decision
{
    Positive,
    Negative,
}