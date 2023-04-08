using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text.Json;

namespace Common.Kafka;

internal class KafkaWorker<TEvent> : BackgroundService
    where TEvent : INotification
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;

    public KafkaWorker(IServiceScopeFactory serviceScopeFactory, IOptions<ConsumerConfig> consumerConfig, ILogger<KafkaWorker<TEvent>> logger)
    {
        _consumerConfig = consumerConfig.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Factory.StartNew(async () =>
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await SubscribeEventAsync<TEvent>(stoppingToken);

        }, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        await Task.CompletedTask;
    }

    public async Task SubscribeEventAsync<TEvent>(CancellationToken cancellationToken) where TEvent : INotification
    {
        using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var topic = typeof(TEvent).GetEventEnvelopeAttribute() ?? throw new ArgumentNullException(nameof(EventEnvelopeAttribute));

        await CreateTopics(topic.Topic);

        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();

        consumer.Subscribe(topic.Topic);

        try
        {
            var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
                await ConsumeNextEvent(consumer, mediator, cancellationToken);
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
    }


    private async Task ConsumeNextEvent(IConsumer<Ignore, string> consumer, IMediator mediator, CancellationToken cancellationToken)
    {
        //IReadOnlyCollection<ConsumeResult<Ignore, string>> consumerResults;
        //try
        //{
        //    consumerResults = consumer!.ConsumeBatch(cancellationToken, 5);
        //    if (!consumerResults.Any()) return;
        //}
        //catch (ConsumeException e) when (new[] { ErrorCode.GroupLoadInProgress }.Contains(e.Error.Code))
        //{
        //    //TODO logger
        //    return;
        //}

        //var parallelOptions = new ParallelOptions
        //{
        //    CancellationToken = cancellationToken,
        //    MaxDegreeOfParallelism = 5,
        //};

        //await Parallel.ForEachAsync(consumerResults, parallelOptions, async (consumerResult, cancellationToken) => await ConsumeResult(consumerResult, consumer, mediator, cancellationToken));


        ConsumeResult<Ignore, string> consumerResult;
        try
        {
            consumerResult = consumer!.Consume(cancellationToken);
            if (consumerResult is null) return;
        }
        catch (ConsumeException e) when (new[] { ErrorCode.GroupLoadInProgress }.Contains(e.Error.Code))
        {
            //TODO logger
            return;
        }

        await ConsumeResult(consumerResult, consumer, mediator, cancellationToken);

        static async ValueTask ConsumeResult(ConsumeResult<Ignore, string> consumerResult, IConsumer<Ignore, string> consumer, IMediator mediator, CancellationToken cancellationToken)
        {
            if (consumerResult.IsPartitionEOF || consumerResult.Message.Value == null) return;


            var start = Stopwatch.GetTimestamp();
            using var activity = Diagnostics.Consumer.Start(consumerResult.Topic, consumerResult.Message);

            try
            {
                activity?.AddDefaultOpenTelemetryTags(consumerResult.Topic, consumerResult.Message);

                var @event = JsonSerializer.Deserialize(consumerResult.Message.Value, typeof(TEvent), JsonSerializerCustomOptions.CamelCase);
                if (@event == null) return;

                await mediator.Publish(@event, cancellationToken);

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
}

internal static class ConsumerExtensions
{
    public static IReadOnlyCollection<ConsumeResult<TKey, TValue>> ConsumeBatch<TKey, TValue>(this IConsumer<TKey, TValue> consumer, CancellationToken cancellationToken, int maxBatchSize)
    {
        var message = consumer.Consume(cancellationToken);

        if (message?.Message is null)
            return Array.Empty<ConsumeResult<TKey, TValue>>();

        var messageBatch = new List<ConsumeResult<TKey, TValue>> { message };

        while (messageBatch.Count < maxBatchSize)
        {
            message = consumer.Consume(TimeSpan.Zero);
            if (message?.Message is null)
                break;

            messageBatch.Add(message);
        }

        return messageBatch;
    }
}

