using Aspire.Hosting.EntityFrameworkCore;
using Npgsql;

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
    
    public static IResourceBuilder<T> WithPostgresReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<PostgresDatabaseResource> source) where T : IResourceWithEnvironment
    {
        return builder
            .WithReference(source, "Postgres")
            .WithEnvironment("DatabaseProvider", "postgres");
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
            IResourceBuilder<PostgresDatabaseResource> postgresResource => WithPostgresReference(builder, postgresResource),
            _ => throw new NotSupportedException("Not supported database"),
        };
    }
}

public static class EFMigrationsExtensions
{
    public static IResourceBuilder<ProjectResource> WithEFMigrations(
        this IResourceBuilder<ProjectResource> project,
        string migrationResourceName,
        string baseDbContextTypeName,
        IResourceBuilder<IResource> database,
        string databaseProvider)
    {
        // EF migrations don't apply to MongoDB (no migrations) or Cosmos (uses EnsureCreated).
        if (string.Equals(databaseProvider, "mongodb", StringComparison.OrdinalIgnoreCase))
            return project;

        // Pick the design-time-only DbContext subclass matching the active provider so
        // dotnet-ef loads the correct per-provider migration track.
        var dbContextTypeName = databaseProvider switch
        {
            "postgres" => $"{baseDbContextTypeName}Postgres",
            _          => $"{baseDbContextTypeName}SqlServer",
        };

        var migrations = project
            .AddEFMigrations(migrationResourceName, dbContextTypeName)
            .WaitFor(database)
            .RunDatabaseUpdateOnStart();

        return project.WaitForCompletion(migrations);
    }
}
