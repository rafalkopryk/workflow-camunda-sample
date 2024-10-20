public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithKafkaReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<KafkaServerResource> source, string groupid, int? port = 9092) where T : IResourceWithEnvironment
    {
        return builder
            .WithReference(source, "Kafka")
            .WithEnvironment("Kafka__groupid", groupid)
            .WithEnvironment("Kafka__enableautocommit", "false")
            .WithEnvironment("Kafka__statisticsintervalms", "5000")
            .WithEnvironment("Kafka__autooffsetreset", "earliest")
            .WithEnvironment("Kafka__enablepartitioneof", "true")
            .WithEnvironment("ServiceBusProvider", "kafka");
    }

    public static IResourceBuilder<T> WithMongoReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<MongoDBServerResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithReference(source, "MongoDB")
            .WithEnvironment("DatabaseProvider", "mongodb");
    }
}
