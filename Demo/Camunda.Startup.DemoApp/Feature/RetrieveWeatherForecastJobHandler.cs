using Camunda.Orchestration.Sdk;
using Camunda.Startup.DemoApp.Dtos;
using Camunda.Client.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace Camunda.Startup.DemoApp.Feature;

public class RetrieveWeatherForecastJobHandler(IMemoryCache memoryCache) : IJobHandler
{
    public Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<WeatherForecastState>();

        string[] summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        var forecast = new WeatherForecast
        (
            input.RequestedDate,
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );

        memoryCache.Set($"WeatherForecast-{input.RequestedDate:yyyy-MM-dd}", forecast, TimeSpan.FromMinutes(5));

        return Task.CompletedTask;
    }
}

public record WeatherForecastState(DateOnly RequestedDate, DateTimeOffset? RetrievedDateTime = null);
