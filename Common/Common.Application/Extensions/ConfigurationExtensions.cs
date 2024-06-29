namespace Common.Application.Extensions;

using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static bool IsCosmosDb(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("DatabaseProvider")?.ToLower() == "cosmosdb";
    }

    public static bool IsKafka(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("ServiceBusProvider")?.ToLower() == "kafka";
    }

    public static string GetAzServiceBusConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("AzServiceBus");
    }

    public static string GetAzCosmosDBConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("AzCosmosDB");
    }

    public static string GetSqlConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("Default");
    }

    public static string GetkafkaConnectionString(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("EventBus:bootstrapservers");
    }

    public static ConsumerConfig? GetkafkaConsumer(this IConfiguration configuration)
    {
        var section = configuration.GetSection("EventBus");
        var config = new ConsumerConfig
        {
            BootstrapServers = section.GetValue<string>("bootstrapservers"),
            GroupId = section.GetValue<string>("groupid"),
            EnableAutoCommit = section.GetValue<bool>("enableautocommit"),
            StatisticsIntervalMs = section.GetValue<int>("statisticsintervalms"),
            AutoOffsetReset = section.GetValue<AutoOffsetReset>("autooffsetreset"),
            EnablePartitionEof = section.GetValue<bool>("enablepartitioneof"),
        };

        return config;
    }

    public static ProducerConfig? GetkafkaProducer(this IConfiguration configuration)
    {
        var section = configuration.GetSection("EventBus");
        var config = new ProducerConfig
        {
            BootstrapServers = section.GetValue<string>("bootstrapservers"),
            Acks = Acks.All,
            StatisticsIntervalMs = section.GetValue<int>("statisticsintervalms"),
            AllowAutoCreateTopics = true,
        };

        return config;
    }

    public static bool UseAzureMonitor(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("OTEL_EXPORTER_AZUREMONITOR__ENABLED");
    }

    public static string GetAzureMonitorEndpoint(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");
    }

    public static bool UseOtlpExporter(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("OTEL_EXPORTER_OTLP_ENABLED");
    }
}