using Camunda.Orchestration.Sdk;
using Camunda.Startup.DemoApp.Dtos;
using Camunda.Startup.DemoApp.Feature;
using Camunda.Client.Extensions;
using Camunda.Startup.DemoApp.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddHostedService<DeployBPMNDefinitionService>();

builder.Services.AddCamundaClient(options =>
{
    options.Config = new()
    {
       ["CAMUNDA_REST_ADDRESS"] = builder.Configuration.GetConnectionString("camunda"),
    };
});

builder.AddCamundaWorkers();

var app = builder.Build();

app.CreateJobWorker<RetrieveWeatherForecastJobHandler>(new JobWorkerConfig
{
    JobType = "weather-forecast-retrieve:1",
    JobTimeoutMs = 30_000,
    PollTimeoutMs = 10_000,
    MaxConcurrentJobs = 2,
});

app.CreateJobWorker<SendNotificationJobHandler>(new JobWorkerConfig
{
    JobType = "send-notification:1",
    JobTimeoutMs = 30_000,
});


app.MapDefaultEndpoints();

app.MapOpenApi();
app.UseHttpsRedirection();

app.MapPost("/weatherforecast/{requestedDate}", async ([FromRoute] DateOnly requestedDate, CamundaClient messageClient) =>
    {
        await messageClient.PublishMessageAsync(new MessagePublicationRequest
        {
            Name = "Message_WeatherForecastRequestReceived",
            Variables = new WeatherForecastRequestReceived(requestedDate),
            MessageId = Guid.CreateVersion7().ToString(),
            TimeToLive = 60_000,
        });

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

public record WeatherForecastRequestReceived(DateOnly RequestedDate);