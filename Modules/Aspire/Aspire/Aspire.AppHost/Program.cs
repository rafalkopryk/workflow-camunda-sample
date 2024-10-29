var builder = DistributedApplication.CreateBuilder(args);

var elastic = builder.AddElasticsearch("elastic")
    .WithEnvironment("xpack.security.enabled", "false")
    .WithDataVolume("elastic")
    .WithLifetime(ContainerLifetime.Persistent);

var elasticConnectionString = ReferenceExpression.Create(
    $"http://{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{elastic.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}");

// var kibana = builder.AddResource(new ContainerResource("kibana"))
//             .WithHttpEndpoint(port: 5602, targetPort: 5601, "http")
//             .WithImage("kibana/kibana", "8.15.3")
//             .WithImageRegistry("docker.elastic.co")
//             .WithEnvironment("ELASTICSEARCH_HOSTS", elasticConnectionString)
//             .WithVolume("kibana", "/usr/share/kibana/data")
//             .WaitFor(elastic);

var kafka = builder.AddKafka("kafka", 62799)
    .WithDataVolume("kafka")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithKafkaUI(x => x.WithLifetime(ContainerLifetime.Persistent));

var mongo = builder.AddMongoDB("mongodb", 57359)
    .WithDataVolume("mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var zeebe = builder.AddZeebe("zeebe", elasticConnectionString, 8089)
    .WithDataVolume("zeebe")
    .WaitFor(elastic)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithOperate("Operate", elasticConnectionString);

builder.AddProject<Projects.Applications_WebApi>("applications-webapi")
    .WithHttpsEndpoint(63111, 8081, "public", isProxied: true)
    .WithMongoReference(mongo).WaitFor(mongo)
    .WithKafkaReference(kafka, "credit-applications").WaitFor(kafka);

builder.AddProject<Projects.Calculations_WebApi>("calculations-webapi")
    .WithExternalHttpEndpoints()
    .WithMongoReference(mongo).WaitFor(mongo)
    .WithKafkaReference(kafka, "credit-calculations").WaitFor(kafka);

builder.AddProject<Projects.Processes_WebApi>("processes-webapi")
    .WithExternalHttpEndpoints()
    .WithKafkaReference(kafka, "credit-processes").WaitFor(kafka)
    .WithZeebeReference(zeebe).WaitFor(zeebe);

builder.AddProject<Projects.Credit_Front_Server>("credit-front-server");

builder.Build().Run();