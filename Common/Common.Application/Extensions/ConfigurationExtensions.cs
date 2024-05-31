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
        ConsumerConfig? config = null;
        configuration.GetSection("EventBus").Bind(config);
        return config;
    }

    public static bool UseAzureMonitor(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("OTEL_EXPORTER_AZUREMONITOR__ENABLED");
    }

    public static string GetAzureMonitorEndpoint(this IConfiguration configuration)
    {
        return configuration.GetValue<string>("OTEL_EXPORTER_AZUREMONITOR__ENDPOINT");
    }

    public static bool UseOtlpExporter(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("OTEL_EXPORTER_OTLP__ENABLED");
    }
}