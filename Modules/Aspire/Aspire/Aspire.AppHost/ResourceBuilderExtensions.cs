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
   
    public static IResourceBuilder<T> WithMongoReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<MongoDBDatabaseResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithReference(source, "MongoDB")
            .WithEnvironment("DatabaseProvider", "mongodb");
    }
    
    public static IResourceBuilder<T> WithSqlServerReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<SqlServerDatabaseResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithReference(source, "Default")
            .WithEnvironment("DatabaseProvider", "sql");
    }

    public static IResourceBuilder<T> WithDatabaseReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> source) where T : IResourceWithEnvironment
    {
        return source switch
        {
            IResourceBuilder<MongoDBDatabaseResource> mongoDbResource =>  WithMongoReference(builder, mongoDbResource),
            IResourceBuilder<SqlServerDatabaseResource> sqlServerResource =>  WithSqlServerReference(builder, sqlServerResource),
            _ => throw new NotSupportedException("Not supported database"),
        };
    }
}
