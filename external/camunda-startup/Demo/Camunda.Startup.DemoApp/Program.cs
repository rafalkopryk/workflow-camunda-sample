using Camunda.Client;
using Camunda.Client.Jobs;
using Camunda.Client.Messages;
using Camunda.Startup.DemoApp.Dtos;
using Camunda.Startup.DemoApp.Feature;
using Camunda.Startup.DemoApp.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddHostedService<DeployBPMNDefinitionService>();

var jobWorkerDefault = new JobWorkerConfiguration();
builder.Services.AddCamunda(
    options =>
    {
        options.Endpoint = builder.Configuration.GetConnectionString("camunda");
    },
    builder => builder
        .AddWorker<RetrieveWeatherForecastJobHandler>(jobWorkerDefault!));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.UseHttpsRedirection();

app.MapPost("/weatherforecast/{requestedDate}", async ([FromRoute] DateOnly requestedDate, IMessageClient messageClient) =>
{
    await messageClient.Publish(new WeatherForecastRequestReceived(requestedDate), Guid.NewGuid().ToString());

    return TypedResults.Accepted(string.Empty);
})
.WithName("StartWeatherForecast");

app.MapGet("/weatherforecast/{requestedDate}", IResult ([FromRoute] DateOnly requestedDate, IMemoryCache memoryCache) =>
{
    return memoryCache.TryGetValue<WeatherForecast>($"WeatherForecast-{requestedDate:yyyy-MM-dd}", out var outValue)
        ? TypedResults.Ok(outValue)
        : TypedResults.NotFound();
})
.WithName("GetWeatherForecast");

app.Run();

[CamundaMessage(Name = "Message_WeatherForecastRequestReceived", TimeToLiveInMs = 60_000)]
public record WeatherForecastRequestReceived(DateOnly RequestedDate);