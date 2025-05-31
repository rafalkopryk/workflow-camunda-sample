namespace Camunda.Client.Messages;

public interface IMessageClient
{
    Task Publish<T>(string correlationKey, T message, string? messageId = null);
}