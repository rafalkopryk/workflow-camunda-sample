using CamundaStartup.Aspire.Hosting.Camunda;

var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka();
var databaseProvider = builder.GetParameter<string>("databaseProvider");
var databaseServer = builder.AddDatabaseServer();
var applicationDatabase = databaseServer.AddDatabaseInstance("credit-applications");
var calculationsDatabase = databaseServer.AddDatabaseInstance("credit-calculations");
var processesDatabase = databaseServer.AddDatabaseInstance("credit-processes");

var application = builder.AddProject<Projects.Applications_WebApi>("applications-webapi")
    .WithDatabaseReference(applicationDatabase).WaitFor(applicationDatabase)
    .WithKafkaReference(kafka, "credit-applications").WaitFor(kafka)
    .WithEFMigrations(
        "applications-migrations",
        "Applications.WebApi.CreditApplicationDbContext",
        applicationDatabase,
        databaseProvider);

builder.AddProject<Projects.Calculations_WebApi>("calculations-webapi")
    .WithExternalHttpEndpoints()
    .WithDatabaseReference(calculationsDatabase).WaitFor(calculationsDatabase)
    .WithKafkaReference(kafka, "credit-calculations").WaitFor(kafka)
    .WithEFMigrations(
        "calculations-migrations",
        "Calculations.WebApi.CreditCalculationDbContext",
        calculationsDatabase,
        databaseProvider);

builder.AddProcess()
    .WithKafkaReference(kafka, "credit-processes").WaitFor(kafka)
    .WithDatabaseReference(processesDatabase).WaitFor(processesDatabase);

builder.AddCreditFront()
    .WithReferenceRelationship(application);

builder.Build().Run();

public static class ProgramExtensions
{
    public static IResourceBuilder<ProjectResource> AddProcess(this IDistributedApplicationBuilder builder)
    {
        var processProvider = builder.GetParameter<string>("processProvider");
        return processProvider switch
        {
            "conductor" => AddProcessesConductorWebApi(),
            "temporal" => AddProcessesTemporalWebApi(),
            "elsa" => AddProcessesElsaWebApi(),
            "saga" => builder.AddProject<Projects.Processes_Saga_WebApi>("processes-saga-webapi")
                .WithExternalHttpEndpoints(),
            _ => AddProcessesCamundaWebApi(),
        };

        IResourceBuilder<ProjectResource> AddProcessesConductorWebApi()
        {
            var conductor = builder.AddContainer("conductor", "conductoross/conductor", "latest")
                .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
                .WithHttpHealthCheck("/health")
                .WithLifetime(ContainerLifetime.Persistent);

            return builder.AddProject<Projects.Processes_Conductor_WebApi>("processes-conductor-webapi")
                .WithExternalHttpEndpoints()
                .WithReference(conductor.GetEndpoint("http"))
                .WithEnvironment("CONDUCTOR_REST_ADDRESS", conductor.GetEndpoint("http"))
                .WaitFor(conductor);
        }

        IResourceBuilder<ProjectResource> AddProcessesTemporalWebApi()
        {
            var temporal = builder.AddTemporalServerContainer("temporal");
            return builder.AddProject<Projects.Processes_Temporal_WebApi>("processes-temporal-webapi")
                .WithReference(temporal)
                .WithExternalHttpEndpoints()
                .WaitFor(temporal);
        }

        IResourceBuilder<ProjectResource> AddProcessesElsaWebApi()
        {
            var elsa = builder.AddProject<Projects.Processes_Elsa_WebApi>("processes-elsa-webapi")
                .WithExternalHttpEndpoints();

            return elsa;
        }
        
        IResourceBuilder<ProjectResource>  AddProcessesCamundaWebApi()
        {
            var camunda = builder.UseCamunda();
            return builder.AddProject<Projects.Processes_Camunda_WebApi>("processes-camunda-webapi")
                .WithExternalHttpEndpoints()
                .WithReference(camunda)
                .WithEnvironment("CAMUNDA_REST_ADDRESS", camunda)
                .WaitFor(camunda);
        }
    }

    public static IResourceBuilder<IResource> AddCreditFront(this IDistributedApplicationBuilder builder)
    {
        var frontProvider = builder.GetParameter<string>("creditFrontProvider");
#pragma warning disable ASPIREJAVASCRIPT001
        return frontProvider == "react"
            ? builder.AddNextJsApp("credit-front-nextjs", "../../../Front/credit.front.next")
                .WithExternalHttpEndpoints()
            : builder.AddProject<Projects.Credit_Front_Blazor>("credit-front-blazor");
#pragma warning restore ASPIREJAVASCRIPT001
    }

    public static IResourceBuilder<IResource> AddDatabaseServer(this IDistributedApplicationBuilder builder)
    {
        var databaseProvider = builder.GetParameter<string>("databaseProvider");
        var databasePassword = builder.AddParameter("databasePassword", secret: true);
        return databaseProvider switch
        {
            "postgres" => builder.AddPostgres("Postgres", port: 5432, password: databasePassword)
                .WithDataVolume("postgres")
                .WithLifetime(ContainerLifetime.Persistent),
            "mongodb" => builder.AddMongoDB("MongoDB", 57359, password: databasePassword)
                .WithDataVolume("mongo")
                .WithLifetime(ContainerLifetime.Persistent),
            _ => builder.AddSqlServer("SqlServer", databasePassword, 62448)
                .WithImageTag("2025-latest")
                .WithDataVolume("sqlserver")
                .WithLifetime(ContainerLifetime.Persistent),
        };
    }

    public static IResourceBuilder<IResource> AddDatabaseInstance(this IResourceBuilder<IResource> resourceBuilder, string database)
    {
        return resourceBuilder switch
        {
            IResourceBuilder<MongoDBServerResource> mongoDbResource => mongoDbResource.AddDatabase(database),
            IResourceBuilder<SqlServerServerResource> sqlServerResource => sqlServerResource.AddDatabase(database),
            IResourceBuilder<PostgresServerResource> postgresResource => postgresResource.AddDatabase(database),
            _ => throw new NotSupportedException("Not supported database server"),
        };
    }

    public static IResourceBuilder<KafkaServerResource> AddKafka(this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka", 62799)
            .WithDataVolume("kafka")
            .WithLifetime(ContainerLifetime.Persistent);

        var kafkaUiEnabled = builder.GetParameter<bool>("kafkaUiEnabled");
        return !kafkaUiEnabled
            ? kafka
            : kafka.WithKafkaUI(x => x.WithLifetime(ContainerLifetime.Persistent));
    }

    public static IResourceBuilder<CamundaResource> UseCamunda(this IDistributedApplicationBuilder builder)
    {
        var h2Password = builder.AddParameter("h2Password", "", secret: true);
        var h2JdbcUrl = ReferenceExpression.Create(
            $"jdbc:h2:file:/usr/local/camunda/data/h2/camunda;DB_CLOSE_DELAY=-1;AUTO_SERVER=TRUE");

        var camunda = builder.AddCamunda("camunda", 8080)
            .WithDataVolume("camunda")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithRdmbsDatabase(h2JdbcUrl, ReferenceExpression.Create($"sa"), h2Password.Resource);

        return camunda;
    }
}
