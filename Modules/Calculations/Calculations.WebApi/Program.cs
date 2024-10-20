using Calculations.Application.Extensions;
using Common.Application.Extensions;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddOpenApi();

builder.Host.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Calculations Api"));

app.UseHttpsRedirection();

await app.ConfigureApplication();

await app.RunAsync();
