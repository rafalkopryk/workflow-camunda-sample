using Camunda.Client.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Processes.Application.Utils;
using Processes.Application.Utils.Importer.File;

internal class DeployBPMNDefinitionService(ICamundaClientRest client, IBpmnProvider provider, IOptions<ProcessDefinitionOptions> processDefinitionsOptions, ILogger<DeployBPMNDefinitionService> logger) : IHostedService
{
    private readonly ProcessDefinitionOptions _processDefinitionsOptions = processDefinitionsOptions.Value;
    private readonly ILogger _logger = logger;
        
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                foreach (var processDefinition in _processDefinitionsOptions.ProcessDefinitions)
                {
                    var file = await provider.GetBpmn(processDefinition);

                    using var memoryStream = new MemoryStream(file, writable: false);

                    FileParameter[] paramers = [ new FileParameter(memoryStream, processDefinition.Name + ".bpmn")];
                    var response = await client.DeploymentsAsync(paramers, string.Empty);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}