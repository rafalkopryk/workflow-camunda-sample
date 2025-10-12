using Camunda.Client.Jobs;
using Camunda.Startup.DemoApp.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace Camunda.Startup.DemoApp.Feature;

[JobWorker(Type = "weather-forecast-retrieve:1")]
public class RetrieveWeatherForecastJobHandler(IMemoryCache memoryCache) : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var input = job.GetVariablesAsType<WeatherForecastState>();

        string[] summaries = [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        var forecast = new WeatherForecast
        (
            input.RequestedDate,
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );

        memoryCache.Set($"WeatherForecast-{input.RequestedDate:yyyy-MM-dd}", forecast, TimeSpan.FromMinutes(5));
        
        //await client.CompleteJobCommand(job);
    }
}

public record WeatherForecastState(DateOnly RequestedDate, DateTimeOffset? RetrievedDateTime = null);
