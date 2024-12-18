﻿namespace Common.Application.Extensions;

using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static bool IsCosmosDb(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("DatabaseProvider")?.ToLower() == "cosmosdb";
    }

    public static bool IsMongoDb(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("DatabaseProvider")?.ToLower() == "mongodb";
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

    public static string GetMongoDbConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("MongoDB");
    }

    public static string GetkafkaConnectionString(this IConfiguration configuration)
    {
        return configuration.GetConnectionString("Kafka");
    }

    public static ConsumerConfig? GetkafkaConsumer(this IConfiguration configuration)
    {
        var bootstrapServers = configuration.GetkafkaConnectionString();
        var section = configuration.GetSection("Kafka");
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
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
        var bootstrapServers = configuration.GetkafkaConnectionString();

        var section = configuration.GetSection("Kafka");
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
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