using Aspire.AppHost;
using CamundaStartup.Aspire.Hosting.Camunda;

var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka();
var databaseServer = builder.AddDatabaseServer();
var applicationDatabase = databaseServer.AddDatabaseInstance("credit-applications");
var calculationsDatabase = databaseServer.AddDatabaseInstance("credit-calculations");
var processesDatabase = databaseServer.AddDatabaseInstance("credit-processes");

var application = builder.AddProject<Projects.Applications_WebApi>("applications-webapi")
    .WithDatabaseReference(applicationDatabase).WaitFor(applicationDatabase)
    .WithKafkaReference(kafka, "credit-applications").WaitFor(kafka);

builder.AddProject<Projects.Calculations_WebApi>("calculations-webapi")
    .WithExternalHttpEndpoints()
    .WithDatabaseReference(calculationsDatabase).WaitFor(calculationsDatabase)
    .WithKafkaReference(kafka, "credit-calculations").WaitFor(kafka);

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
            "temporal" => AddProcessesTemporalWebApi(),
            "saga" => builder.AddProject<Projects.Processes_Saga_WebApi>("processes-saga-webapi")
                .WithExternalHttpEndpoints(),
            _ => AddProcessesCamundaWebApi(),
        };

        IResourceBuilder<ProjectResource> AddProcessesTemporalWebApi()
        {
            var temporal = builder.AddTemporalServerContainer("temporal");
            return builder.AddProject<Projects.Processes_Temporal_WebApi>("processes-temporal-webapi")
                .WithReference(temporal)
                .WithExternalHttpEndpoints()
                .WaitFor(temporal);
        }
        
        IResourceBuilder<ProjectResource>  AddProcessesCamundaWebApi()
        {
            var camunda = builder.UseCamunda();
            return builder.AddProject<Projects.Processes_WebApi>("processes-webapi")
                .WithExternalHttpEndpoints()
                .WithCamundaReference(camunda).WaitFor(camunda);
        }
    }

    public static IResourceBuilder<IResource> AddCreditFront(this IDistributedApplicationBuilder builder)
    {
        var frontProvider = builder.GetParameter<string>("creditFrontProvider");
        return frontProvider == "react"
            ? builder.AddJavaScriptApp("credit-front-nextjs", "../../../Front/credit.front.next")
                .WithRunScript("dev")
                .WithHttpEndpoint(env: "PORT", port: 3000)
                .WithExternalHttpEndpoints()
            : builder.AddProject<Projects.Credit_Front_Blazor>("credit-front-blazor");
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
        var elastic = builder.AddElasticsearch("elastic")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithDataVolume("elastic")
            .WithLifetime(ContainerLifetime.Persistent);

        var elasticConnectionString = ReferenceExpression.Create(
            $"http://{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}");

        var camunda = builder.AddCamunda("camunda", port: 8089, elasticConnectionString)
            .WithDataVolume("camunda")
            .WithLifetime(ContainerLifetime.Persistent);

        var kibanaEnabled = builder.GetParameter<bool>("kibanaEnabled");
        if (kibanaEnabled)
        {
            var kibana = builder.AddResource(new ContainerResource("kibana"))
                .WithHttpEndpoint(port: 5602, targetPort: 5601, "http")
                .WithImage("kibana/kibana", "8.17.3")
                .WithImageRegistry("docker.elastic.co")
                .WithEnvironment("ELASTICSEARCH_HOSTS", elasticConnectionString)
                .WithVolume("kibana", "/usr/share/kibana/data")
                .WaitFor(elastic);
        }

        return camunda;
    }
}
