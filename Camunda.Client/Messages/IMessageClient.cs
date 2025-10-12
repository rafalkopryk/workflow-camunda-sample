namespace Camunda.Client.Messages;

public interface IMessageClient
{
    Task Publish<T>(string correlationKey, T message, string? messageId = null);

    Task Publish<T>(T message, string? messageId = null) => Publish(null, message, messageId);
}