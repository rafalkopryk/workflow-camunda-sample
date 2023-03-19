using Camunda.Connector.Kafka.Inbound.Model;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace Camunda.Connector.Kafka.Inbound;

internal class KafkaSubscription : IKafkaSubscription
{
    private readonly ILogger _logger;

    private readonly ConsumerConfig _consumerConfig;

    public KafkaSubscription(ILogger<KafkaSubscription> logger, IOptions<ConsumerConfig> consumerConfig)
    {
        _logger = logger;
        _consumerConfig = consumerConfig.Value;

        _consumerConfig.AllowAutoCreateTopics ??= true;
        _consumerConfig.EnableAutoCommit ??= false;
        _consumerConfig.AutoOffsetReset ??= AutoOffsetReset.Earliest;
    }

    public async Task ProduceEvent(KafkaProperties properties, Func<KafkaSubscriptionEvent, Task> callback, CancellationToken cancellationToken)
    {
        await CreateTopics(properties.SubscriptionTopic);

        _ = Task.Factory.StartNew(async () =>
        {
            using var consumer = new ConsumerBuilder<Ignore, byte[]>(_consumerConfig).Build();
            consumer.Subscribe(properties.SubscriptionTopic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    await ConsumeNextEvent(callback, consumer, TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, "Error consuming message");
                consumer.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                consumer.Assign(consumer.Assignment);
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        await Task.CompletedTask;
    }

    private async Task CreateTopics(params string[] topics)
    {
        try
        {
            var config = new AdminClientConfig(_consumerConfig);
            using var adminClient = new AdminClientBuilder(config).Build();

            await adminClient.CreateTopicsAsync(topics.Select(topic => new TopicSpecification
            {
                Name = topic
            }));
        }
        catch (CreateTopicsException ex) when (ex.Message.Contains("already exists"))
        {
        }
    }

    private async Task ConsumeNextEvent(
        Func<KafkaSubscriptionEvent, Task> callback,
        IConsumer<Ignore, byte[]> consumer,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ConsumeResult<Ignore, byte[]> consumerResult;
        try
        {
            consumerResult = consumer!.Consume(timeout);
            if (consumerResult == null) return;
        }
        catch (ConsumeException e) when (new[] { ErrorCode.GroupLoadInProgress }.Contains(e.Error.Code))
        {
            //TODO logger
            return;
        }

        if (consumerResult.IsPartitionEOF || consumerResult.Message.Value == null) return;

        var message = JsonSerializer.Deserialize<Dictionary<string, object>>(consumerResult.Message.Value, JsonSerializerKafkaOptions.CamelCase);
        var subscriptionEvent = new KafkaSubscriptionEvent(string.Empty, 000, message);

        await callback.Invoke(subscriptionEvent);

        consumer.Commit(consumerResult);
    }
}
