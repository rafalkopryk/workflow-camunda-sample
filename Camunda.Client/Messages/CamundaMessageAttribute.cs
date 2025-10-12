namespace Camunda.Client.Messages;

[AttributeUsage(AttributeTargets.Class)]
public class CamundaMessageAttribute : Attribute
{
    public required string Name { get; init; }
    public long TimeToLiveInMs { get; init; } = 24 * 3600 * 1000;
}
