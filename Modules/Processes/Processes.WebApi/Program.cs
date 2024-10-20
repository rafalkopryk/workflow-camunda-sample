using Common.Application.Extensions;
using OpenTelemetry.Resources;
using Processes.Application.Extensions;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = true;
    options.ServicesStopConcurrently = false;
});

builder.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Processes Api"));

app.UseHttpsRedirection();

app.Run();
