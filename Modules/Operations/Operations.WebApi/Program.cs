using System.Text.Json.Serialization;
using System.Text.Json;

using Common.Application.Extensions;
using Operations.Application.Extensions;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("Credit.Operations", serviceVersion: "1.0.0")
    .AddTelemetrySdk();

builder.Logging.ConfigureLogger(builder.Configuration, resourceBuilder);
builder.Services.AddInfrastructure(builder.Configuration, resourceBuilder);
builder.Services.AddApplication(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

app.UseAuthorization();

app.MapControllers();

app.Run();
