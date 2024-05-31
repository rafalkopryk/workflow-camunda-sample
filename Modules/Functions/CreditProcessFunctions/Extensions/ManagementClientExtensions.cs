using Microsoft.Azure.ServiceBus.Management;

public static class ManagementClientExtensions
{
    public static async Task<bool> TryCreateTopic(this ManagementClient client, string topic)
    {
        if (!await client.TopicExistsAsync(topic))
        {
            try
            {
                await client.CreateTopicAsync(topic);
                return true;
            }
            catch (MessagingEntityAlreadyExistsException ex)
            {
                return false;
            }
        }

        return false;
    }

    public static async Task<bool> TryCreateSubcription(this ManagementClient client, string topicPath, string subscription)
    {
        if (!await client.SubscriptionExistsAsync(topicPath, subscription))
        {
            try
            {
                await client.CreateSubscriptionAsync(topicPath, subscription);
                return true;
            }
            catch (MessagingEntityAlreadyExistsException ex)
            {
                return false;
            }
        }

        return false;
    }
}