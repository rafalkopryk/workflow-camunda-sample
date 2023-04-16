using System.Reflection;

namespace Common.Kafka;

public static class EventEnvelopeExtensions
{
    public static EventEnvelopeAttribute? GetEventEnvelopeAttribute(this Type input)
    {
        var result = input.GetCustomAttributes<EventEnvelopeAttribute>().FirstOrDefault();
        return result;
    }
}
