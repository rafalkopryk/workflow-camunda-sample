using Common.Application.Extensions;
using OpenTelemetry.Resources;
using Processes.Application.Extensions;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = false;
});

builder.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Credit.Processes", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

builder.Services.AddInfrastructure(builder.Configuration, resourceBuilder);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Processes Api"));

app.UseHttpsRedirection();

app.Run();
