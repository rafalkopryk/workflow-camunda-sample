namespace Camunda.Client;

[AttributeUsage(AttributeTargets.Class)]
public class ZeebeMessageAttribute : Attribute
{
    public required string Name { get; init; }
    public long TimeToLiveInMs { get; init; }
}

