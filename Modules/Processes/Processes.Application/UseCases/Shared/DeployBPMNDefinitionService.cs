using GatewayProtocol;
using Google.Protobuf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Processes.Application.Utils;
using Processes.Application.Utils.Importer.File;

internal class DeployBPMNDefinitionService : IHostedService
{
    private readonly Gateway.GatewayClient _client;
    private readonly IBpmnProvider _provider;
    private readonly ProcessDefinitionOptions _processDefinitionsOptions;

    public DeployBPMNDefinitionService(Gateway.GatewayClient client, IBpmnProvider provider, IOptions<ProcessDefinitionOptions> processDefinitionsOptions)
    {
        _client = client;
        _provider = provider;
        _processDefinitionsOptions = processDefinitionsOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        try
        {
            foreach (var processDefinition in _processDefinitionsOptions.ProcessDefinitions)
            {
                var file = await _provider.GetBpmn(processDefinition);
                _ = await _client.DeployResourceAsync(new DeployResourceRequest
                {
                    Resources =
                    {
                        new Resource
                        {
                            Name = processDefinition.Name + ".bpmn",
                            Content = ByteString.CopyFrom(file),
                        }
                    }
                });
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