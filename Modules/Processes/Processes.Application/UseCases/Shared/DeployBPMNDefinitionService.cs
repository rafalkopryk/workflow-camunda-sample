using Camunda.Client.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Processes.Application.Utils;
using Processes.Application.Utils.Importer.File;

internal class DeployBPMNDefinitionService(ICamundaClientRest client, IBpmnProvider provider, IOptions<ProcessDefinitionOptions> processDefinitionsOptions) : IHostedService
{
    private readonly ICamundaClientRest _client = client;
    private readonly IBpmnProvider _provider = provider;
    private readonly ProcessDefinitionOptions _processDefinitionsOptions = processDefinitionsOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        try
        {
            foreach (var processDefinition in _processDefinitionsOptions.ProcessDefinitions)
            {
                var file = await _provider.GetBpmn(processDefinition);

                using var memoryStream = new MemoryStream(file, writable: false);

                FileParameter[] paramers = [ new FileParameter(memoryStream, processDefinition.Name + ".bpmn")];
                var response = await _client.DeploymentsAsync(paramers, string.Empty);
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}