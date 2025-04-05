var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka();
var databaseServer = builder.AddDatabaseServer();
var applicationDatabase = databaseServer.AddDatabaseInstance("credit-applications");
var calculationsDatabase = databaseServer.AddDatabaseInstance("credit-calculations");


//var azureServiceBus = builder.AddAzureServiceBus("servicebus")
//    .RunAsEmulator();


builder.AddProject<Projects.Applications_WebApi>("applications-webapi")
    .WithHttpsEndpoint(63111, 8081, "public", isProxied: true)
    .WithDatabaseReference(applicationDatabase).WaitFor(applicationDatabase)
    .WithKafkaReference(kafka, "credit-applications").WaitFor(kafka);
    //.WithAzureServiceBusReference(azureServiceBus).WaitFor(azureServiceBus);

builder.AddProject<Projects.Calculations_WebApi>("calculations-webapi")
    .WithExternalHttpEndpoints()
    .WithDatabaseReference(calculationsDatabase).WaitFor(calculationsDatabase)
    .WithKafkaReference(kafka, "credit-calculations").WaitFor(kafka);
    //.WithAzureServiceBusReference(azureServiceBus).WaitFor(azureServiceBus);


builder.AddProcess()
    .WithKafkaReference(kafka, "credit-processes").WaitFor(kafka)
    //.WithAzureServiceBusReference(azureServiceBus).WaitFor(azureServiceBus)
    .WithDatabaseReference(applicationDatabase).WaitFor(applicationDatabase);

builder.AddCreditFront();

builder.Build().Run();

public static class ProgramExtensions
{
    public static IResourceBuilder<ProjectResource> AddProcess(this IDistributedApplicationBuilder builder)
    {
        var processProvider = builder.AddParameter("processProvider");
        if (processProvider.Resource.Value == "saga")
        {
            return builder.AddProject<Projects.Processes_Saga_WebApi>("processes-saga-webapi")
                .WithExternalHttpEndpoints();
        }

        var camunda = builder.AddCamunda();
        return builder.AddProject<Projects.Processes_WebApi>("processes-webapi")
            .WithExternalHttpEndpoints()
            .WithZeebeReference(camunda).WaitFor(camunda);
    }

    public static IResourceBuilder<IResource> AddCreditFront(this IDistributedApplicationBuilder builder)
    {
        var frontProvider = builder.AddParameter("creditFrontProvider");
        return frontProvider.Resource.Value == "react"
            ? builder.AddNpmApp("credit-front-nextjs", "../../../Front/credit.front.next", "dev")
                .WithHttpEndpoint(env: "PORT", port: 3000)
                .WithExternalHttpEndpoints()
            : builder.AddProject<Projects.Credit_Front_Blazor>("credit-front-blazor");
    }

    public static IResourceBuilder<IResource> AddDatabaseServer(this IDistributedApplicationBuilder builder)
    {
        var databaseProvider = builder.AddParameter("databaseProvider");
        var databasePassword = builder.AddParameter("databasePassword", secret: true);
        return databaseProvider.Resource.Value == "mongodb"
            ? builder.AddMongoDB("MongoDB", 57359, password: databasePassword)
                .WithDataVolume("mongo")
                .WithLifetime(ContainerLifetime.Persistent)
            : builder.AddSqlServer("SqlServer", databasePassword, 62448)
                .WithDataVolume("sqlserver")
                .WithLifetime(ContainerLifetime.Persistent);
    }

    public static IResourceBuilder<IResource> AddDatabaseInstance(this IResourceBuilder<IResource> resourceBuilder, string database)
    {
        return resourceBuilder switch
        {
            IResourceBuilder<MongoDBServerResource> mongoDbResource => mongoDbResource.AddDatabase(database),
            IResourceBuilder<SqlServerServerResource> sqlServerResource => sqlServerResource.AddDatabase(database),
            _ => throw new NotSupportedException("Not supported database server"),
        };
    }

    public static IResourceBuilder<KafkaServerResource> AddKafka(this IDistributedApplicationBuilder builder)
    {
        var kafka = builder.AddKafka("kafka", 62799)
            .WithDataVolume("kafka")
            .WithLifetime(ContainerLifetime.Persistent);

        var kafkaUiEnabled = builder.AddParameter("kafkaUiEnabled");
        return kafkaUiEnabled.Resource.Value != bool.TrueString
            ? kafka
            : kafka.WithKafkaUI(x => x.WithLifetime(ContainerLifetime.Persistent));
    }

    public static IResourceBuilder<CamundaResource> AddCamunda(this IDistributedApplicationBuilder builder)
    {
        var camunda = builder.AddCamunda("camunda", restPort: 8089)
            .WithDataVolume("camunda")
            .WithLifetime(ContainerLifetime.Persistent);

        var operateEnabled = builder.AddParameter("operateEnabled");
        if (operateEnabled.Resource.Value != bool.TrueString)
        {
            return camunda;
        }

        var elastic = builder.AddElasticsearch("elastic")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithDataVolume("elastic")
            .WithLifetime(ContainerLifetime.Persistent);

        var elasticConnectionString = ReferenceExpression.Create(
            $"http://{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}");

        var kibanaEnabled = builder.AddParameter("kibanaEnabled");
        if (kibanaEnabled.Resource.Value == bool.TrueString)
        {
            var kibana = builder.AddResource(new ContainerResource("kibana"))
                .WithHttpEndpoint(port: 5602, targetPort: 5601, "http")
                .WithImage("kibana/kibana", "8.15.3")
                .WithImageRegistry("docker.elastic.co")
                .WithEnvironment("ELASTICSEARCH_HOSTS", elasticConnectionString)
                .WithVolume("kibana", "/usr/share/kibana/data")
                .WaitFor(elastic);
        }

        return camunda.WithCamundaExporter(elasticConnectionString)
            .WaitFor(elastic)
            .WithOperate("Operate", elasticConnectionString)
            .WithDatabase(elasticConnectionString);
    }
}