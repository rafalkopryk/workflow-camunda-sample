using Calculations.Application.Extensions;
using Common.Application.Extensions;
using OpenTelemetry.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Credit.Calculations", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

builder.Services.AddInfrastructure(builder.Configuration, resourceBuilder);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseOpenApi();
app.UseSwaggerUi();
app.UseReDoc(options =>
{
    options.Path = "/redoc";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.ConfigureApplication();

await app.RunAsync();
