using Azure.Messaging.ServiceBus;

namespace CreditProcessFunctions.Extensions;

public static class ServiceBusSenderExtensions
{
    public static async Task SendMessageAsync<T>(this ServiceBusSender sender, T message)
    {
        await sender.SendMessageAsync(new ServiceBusMessage
        {
            ContentType = "application/json",
            Body = BinaryData.FromObjectAsJson(message, CustomJsonSerializerOptions.CamelCase),
        });
    }
}
