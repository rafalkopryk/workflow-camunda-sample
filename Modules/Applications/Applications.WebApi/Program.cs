using Applications.Application.Extensions;
using Common.Application.Extensions;
using OpenTelemetry.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Credit.Applications", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

builder.Services.AddInfrastructure(builder.Configuration, resourceBuilder);
builder.Services.AddApplication(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();

builder.Services.AddCors();

builder.Host.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())

app.UseOpenApi();
app.UseSwaggerUi();
app.UseReDoc(options =>
{
    options.Path = "/redoc";
});

app.UseHttpsRedirection();

app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

app.UseAuthorization();

app.MapControllers();

await app.ConfigureApplication();

await app.RunAsync();
