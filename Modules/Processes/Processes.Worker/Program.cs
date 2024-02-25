using OpenTelemetry.Resources;
using Common.Application.Extensions;
using Processes.Application.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = false;
});

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Credit.Processes", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

builder.Logging.ConfigureLogger(builder.Configuration, resourceBuilder);
builder.Services.AddInfrastructure(builder.Configuration, resourceBuilder);
builder.Services.AddApplication(builder.Configuration);

var configuration = builder.Configuration;

var host = builder.Build();
await host.RunAsync();