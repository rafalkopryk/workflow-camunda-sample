var builder = DistributedApplication.CreateBuilder(args);

var camunda = AddCamunda(builder);
var kafka = AddKafka(builder);
var databaseServer = AddDatabaseServer(builder);
var applicationDatabase = AddDatabase(databaseServer,"credit-applications");
var calculationsDatabase = AddDatabase(databaseServer, "credit-calculations");

builder.AddProject<Projects.Applications_WebApi>("applications-webapi")
    .WithHttpsEndpoint(63111, 8081, "public", isProxied: true)
    .WithDatabaseReference(applicationDatabase).WaitFor(applicationDatabase)
    .WithKafkaReference(kafka, "credit-applications").WaitFor(kafka);

builder.AddProject<Projects.Calculations_WebApi>("calculations-webapi")
    .WithExternalHttpEndpoints()
    .WithDatabaseReference(calculationsDatabase).WaitFor(calculationsDatabase)
    .WithKafkaReference(kafka, "credit-calculations").WaitFor(kafka);

builder.AddProject<Projects.Processes_WebApi>("processes-webapi")
    .WithExternalHttpEndpoints()
    .WithKafkaReference(kafka, "credit-processes").WaitFor(kafka)
    .WithZeebeReference(camunda).WaitFor(camunda);

builder.AddProject<Projects.Credit_Front_Server>("credit-front-server");

builder.Build().Run();

static IResourceBuilder<IResource> AddDatabaseServer(IDistributedApplicationBuilder builder)
{
    var databaseProvider = builder.AddParameter("databaseProvider");
    var databasePassword = builder.AddParameter("databasePassword", secret: true);
    return databaseProvider.Resource.Value == "mongodb"
        ? builder.AddMongoDB("MongoDB", 57359, password: databasePassword)
            .WithDataVolume("mongo")
            .WithLifetime(ContainerLifetime.Persistent)
        : builder.AddSqlServer("SqlServer", databasePassword,62448)
            .WithDataVolume("sqlserver")
            .WithLifetime(ContainerLifetime.Persistent);
}

static IResourceBuilder<IResource> AddDatabase(IResourceBuilder<IResource> resourceBuilder, string database)
{
    return resourceBuilder switch
    {
        IResourceBuilder<MongoDBServerResource> mongoDbResource =>  mongoDbResource.AddDatabase(database),
        IResourceBuilder<SqlServerServerResource> sqlServerResource =>  sqlServerResource.AddDatabase(database),
        _ => throw new NotSupportedException("Not supported database server"),
    };
}

static IResourceBuilder<KafkaServerResource> AddKafka(IDistributedApplicationBuilder builder)
{
    var kafka = builder.AddKafka("kafka", 62799)
        .WithDataVolume("kafka")
        .WithLifetime(ContainerLifetime.Persistent);
    
    var kafkaUiEnabled = builder.AddParameter("kafkaUiEnabled");
    return kafkaUiEnabled.Resource.Value != bool.TrueString
        ? kafka
        : kafka.WithKafkaUI(x => x.WithLifetime(ContainerLifetime.Persistent));
}

static IResourceBuilder<ZeebeResource> AddCamunda(IDistributedApplicationBuilder builder)
{
    var zeebe = builder.AddZeebe("zeebe", restPort: 8089)
        .WithDataVolume("zeebe")
        .WithLifetime(ContainerLifetime.Persistent); 
    
    var operateEnabled = builder.AddParameter("operateEnabled");
    if (operateEnabled.Resource.Value != bool.TrueString)
    {
        return zeebe;
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

    return zeebe.WithElasticExporter(elasticConnectionString)
        .WaitFor(elastic)
        .WithOperate("Operate", elasticConnectionString);
}