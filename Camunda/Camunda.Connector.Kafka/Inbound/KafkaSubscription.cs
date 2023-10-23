using Camunda.Connector.Kafka.Inbound.Model;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace Camunda.Connector.Kafka.Inbound;

internal class KafkaSubscription : IKafkaSubscription
{
    private readonly ILogger _logger;

    private readonly ConsumerConfig _consumerConfig;

    private static readonly ConcurrentDictionary<string, HashSet<Func<KafkaInboundMessage, Task>>> TopicCallbacks = new ();

    public KafkaSubscription(ILogger<KafkaSubscription> logger, IOptions<ConsumerConfig> consumerConfig)
    {
        _logger = logger;
        _consumerConfig = consumerConfig.Value;

        _consumerConfig.AllowAutoCreateTopics ??= true;
        _consumerConfig.EnableAutoCommit ??= false;
        _consumerConfig.AutoOffsetReset ??= AutoOffsetReset.Earliest;
    }

    public async Task Subscribe(KafkaConnectorProperties properties, Func<KafkaInboundMessage, Task> callback, CancellationToken cancellationToken)
    {
        TopicCallbacks.AddOrUpdate(
            properties.TopicName,
            new HashSet<Func<KafkaInboundMessage, Task>> { callback },
            (x, y) =>
            {
                y.Add(callback);
                return y;
            });

        var callbacks = TopicCallbacks.FirstOrDefault(x => x.Key == properties.TopicName).Value.ToArray();

        await CreateTopics(properties.TopicName);

        _ = Task.Factory.StartNew(async () =>
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(properties.TopicName);  // TODO : should we allow multiple topics?

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    await ConsumeNextEvent(callbacks, consumer, TimeSpan.FromSeconds(1), cancellationToken);
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
        Func<KafkaInboundMessage, Task>[] callbacks,
        IConsumer<string, string> consumer,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ConsumeResult<string, string> consumerResult;
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

        var start = Stopwatch.GetTimestamp();
        using var activity = Diagnostics.Consumer.Start(consumerResult.Topic, consumerResult.Message);

        try
        {
            activity?.AddDefaultOpenTelemetryTags(consumerResult.Topic, consumerResult.Message);

            var message = JsonSerializer.Deserialize<Dictionary<string, object>>(consumerResult.Message.Value, JsonSerializerKafkaOptions.CamelCase);
            var kafkaInboundMessage = new KafkaInboundMessage(consumerResult.Key, consumerResult.Message.Value, message);

            foreach (var callback in callbacks)
            {
                await callback.Invoke(kafkaInboundMessage);
            }

            consumer.Commit();

            var tags = new TagList
            {
                new ("topic", consumerResult.Topic),
                new ("status", "Positive")
            };

            Diagnostics.ConsumeCounter.Add(1, tags);
            Diagnostics.ConsumeHistogram.Record(Stopwatch.GetElapsedTime(start).TotalMilliseconds, tags);
        }
        catch (Exception e)
        {
            var tags = new TagList
            {
                new ("topic", consumerResult.Topic),
                new ("status", "Failed")
            };

            Diagnostics.ConsumeCounter.Add(1, tags);
            Diagnostics.ConsumeHistogram.Record(Stopwatch.GetElapsedTime(start).TotalMilliseconds, tags);

            activity?.RecordException(e);
            throw;
        }

        consumer.Commit(consumerResult);
    }
}
