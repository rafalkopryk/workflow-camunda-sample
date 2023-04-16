using Camunda.Connector.SDK.Runtime.Inbound.Lifecycle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer;

internal class ProcessDefinitionImporter : BackgroundService
{
    private readonly ProcessDefinitionOptions _processDefinitionsOptions;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProcessDefinitionImporter(IOptions<ProcessDefinitionOptions> processDefinitionOptions, IServiceScopeFactory serviceScopeFactory)
    {
        _processDefinitionsOptions = processDefinitionOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processDefinitions = _processDefinitionsOptions.ProcessDefinitions;

        using var serviceScope = _serviceScopeFactory.CreateScope();
        var inboundCorrelationHandler = serviceScope.ServiceProvider.GetRequiredService<InboundConnectorManager>();

        await inboundCorrelationHandler.RegisterProcessDefinitions(processDefinitions);
    }
}
