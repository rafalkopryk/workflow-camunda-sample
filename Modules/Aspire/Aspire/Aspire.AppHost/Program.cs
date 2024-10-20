var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("kafka").WithKafkaUI();
var mongo = builder.AddMongoDB("mongodb");
var zeebe = builder.AddZeebe("zeebe", 8089);

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
