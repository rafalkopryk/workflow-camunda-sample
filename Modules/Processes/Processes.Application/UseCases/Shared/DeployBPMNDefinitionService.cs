using Camunda.Orchestration.Sdk;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Processes.Application.Utils;
using Processes.Application.Utils.Importer.File;

internal class DeployBPMNDefinitionService(
    CamundaClient client,
    IBpmnProvider provider,
    IOptions<ProcessDefinitionOptions> processDefinitionsOptions,
    ILogger<DeployBPMNDefinitionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var processDefinition in processDefinitionsOptions.Value.ProcessDefinitions)
                {
                    var file = await provider.GetBpmn(processDefinition);

                    var multipartFormDataContent = new MultipartFormDataContent();
                    multipartFormDataContent.Add(new ByteArrayContent(file), "resources", processDefinition.Name + ".bpmn");

                    await client.CreateDeploymentAsync(multipartFormDataContent);
                }

                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
