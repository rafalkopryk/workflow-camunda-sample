namespace Camunda.Client.Messages;

public interface IMessageClient
{
    Task Publish<T>(string correlationKey, T message, string? messageId = null);

    Task Publish<T>(string name, string correlationKey, T message, long timeToLive, string? messageId = null);

}