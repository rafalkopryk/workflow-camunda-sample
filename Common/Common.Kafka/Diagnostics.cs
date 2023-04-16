namespace Common.Kafka;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using Confluent.Kafka;

internal static class Diagnostics
{
    private const string ActivitySourceName = "Common.Kafka";
    public static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);

    public static readonly Meter Meter = new("Common.Kafka");
    public static readonly Counter<int> ConsumeCounter = Meter.CreateCounter<int>("consume-events-count");
    public static readonly Histogram<double> ConsumeHistogram = Meter.CreateHistogram<double>("consume-events-duration", "ms", "measures the duration of the consume events");

    internal static class Producer
    {
        internal static Activity Start<TKey, TValue>(string topic, Message<TKey, TValue> message)
        {
            var activityName = $"Kafka SEND to {topic}";
            return ActivitySource.StartActivity(activityName, ActivityKind.Producer);
        }
    }

    internal static class Consumer
    {
        internal static Activity Start<TKey, TValue>(string topic, Message<TKey, TValue> message)
        {
            var headers = message.Headers.ToDictionary(x => x.Key, y => Encoding.ASCII.GetString(y.GetValueBytes()));
            var traceparent = headers.FirstOrDefault(x => x.Key == "traceparent").Value;

            var activityName = $"Kafka RECEIVE from {topic}";
            return string.IsNullOrWhiteSpace(traceparent)
                ? ActivitySource.StartActivity(activityName, ActivityKind.Consumer)
                : ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentId: traceparent);
        }
    }

    public static Activity AddDefaultOpenTelemetryTags<TKey, TValue>(
        this Activity activity,
        string topic,
        Message<TKey, TValue> message)
    {
        activity?.AddTag(MessagingAttributes.SYSTEM, "kafka");

        activity?.AddTag(MessagingAttributes.DESTINATION, topic);
        activity?.AddTag(MessagingAttributes.DESTINATION_KIND, "topic");

        if (message.Key != null)
            activity?.AddTag(MessagingAttributes.KAFKA_MESSAGE_KEY, message.Key);

        if (message.Value != null)
        {
            int messagePayloadBytes = Encoding.UTF8.GetByteCount(message.Value.ToString());
            activity?.AddTag(MessagingAttributes.MESSAGE_PAYLOAD_SIZE_BYTES, messagePayloadBytes.ToString());
        }

        return activity;
    }
}
