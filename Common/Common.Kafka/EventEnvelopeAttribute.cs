using System;

namespace Common.Kafka;

[AttributeUsage(AttributeTargets.Class)]
public class EventEnvelopeAttribute : Attribute
{
    public string Topic { get; set; }
}
