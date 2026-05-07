using Camunda.Orchestration.Sdk;

namespace Camunda.Startup.DemoApp.UseCases;

public class DeployBPMNDefinitionService(CamundaClient client, ILogger<DeployBPMNDefinitionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var weatherForecastDefinition = "weather-forecast2.bpmn";

                var file = await File.ReadAllBytesAsync(weatherForecastDefinition, stoppingToken);

                var multipartFormDataContent = new MultipartFormDataContent();
                multipartFormDataContent.Add(new ByteArrayContent(file), "resources", weatherForecastDefinition);
                
                var response = await client.CreateDeploymentAsync(multipartFormDataContent);

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
