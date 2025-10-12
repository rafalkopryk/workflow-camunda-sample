using Camunda.Client.Rest;

namespace Camunda.Startup.DemoApp.UseCases;

public class DeployBPMNDefinitionService(ICamundaClientRest client, ILogger<DeployBPMNDefinitionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var weatherForecastDefinition = "weather-forecast.bpmn";

                var file = await File.ReadAllBytesAsync(weatherForecastDefinition, stoppingToken);
                using var memoryStream = new MemoryStream(file, writable: false);

                FileParameter[] paramers = [new FileParameter(memoryStream, weatherForecastDefinition)];
                var response = await client.DeploymentsAsync(paramers, string.Empty, stoppingToken);

                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
