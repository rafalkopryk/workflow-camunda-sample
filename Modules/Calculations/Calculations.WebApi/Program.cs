using Calculations.Application.Extensions;
using Common.Application.Extensions;
using OpenTelemetry.Resources;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Credit.Calculations", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

builder.Services.AddInfrastructure(builder.Configuration, resourceBuilder);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddOpenApi();

builder.Host.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Calculations Api"));

app.UseHttpsRedirection();

await app.ConfigureApplication();

await app.RunAsync();
