using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Runtime.Inbound.Importer;
using Camunda.Connector.SDK.Runtime.Util.Inbound;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Camunda.Connector.SDK.Runtime.Inbound.Lifecycle;

internal class InboundConnectorManager
{
    private readonly InboundCorrelationHandler _correlationHandler;

    private readonly IInboundConnectorFactoryConnectorFactory _factoryConnectorFactory;

    private readonly IProcessDefinitionInspector _processDefinitionInspector;

    private readonly HashSet<long> _registeredProcessDefinitionKeys = new();

    private readonly ConcurrentDictionary<string, IInboundConnectorExecutable> activeConnectorsByExecutionId = new();

    private readonly ILogger _logger;

    public ConcurrentDictionary<string, HashSet<InboundConnectorProperties>> ActiveConnectorsByBpmnId { get; } = new ();

    public InboundConnectorManager(InboundCorrelationHandler correlationHandler, IInboundConnectorFactoryConnectorFactory factoryConnectorFactory, IProcessDefinitionInspector processDefinitionInspector, ILogger<InboundConnectorManager> logger)
    {
        _correlationHandler = correlationHandler;
        _factoryConnectorFactory = factoryConnectorFactory;
        _processDefinitionInspector = processDefinitionInspector;
        _logger = logger;
    }

    public bool IsProcessDefinitionRegistered(long processDefinitionKey)
    {
        return _registeredProcessDefinitionKeys.Contains(processDefinitionKey);
    }

    public async Task RegisterProcessDefinitions(ProcessDefinition[] processDefinitions)
    {
        if (processDefinitions?.Length > 0 != true)
        {
            return;
        }

        var newProcessDefinitions = processDefinitions
            .Where(processDefinition => !IsProcessDefinitionRegistered(processDefinition.Key))
            .ToArray();
            
        foreach (var processDefinition in newProcessDefinitions)
        {
            _registeredProcessDefinitionKeys.Add(processDefinition.Key);
        }

        var relevantProcessDefinitions = processDefinitions
            .GroupBy(processDefinition => processDefinition.BpmnProcessId)
            .ToArray()
            .Select(defitionsByBpmnProcessId => defitionsByBpmnProcessId.OrderByDescending(y => y.Version).FirstOrDefault());

        foreach (var processDefinition in relevantProcessDefinitions)
        {
            var inboundConnectors = _processDefinitionInspector.FindInboundConnectors(processDefinition);
            await HandleLatestBpmnVersion(processDefinition.BpmnProcessId, inboundConnectors);
        }
    }

    private async Task ActivateConnector(InboundConnectorProperties newProperties, CancellationToken cancellationToken)
    {
        var executable = _factoryConnectorFactory.GetInstance(newProperties.Type);

        try
        {
            await executable.Activate(new InboundConnectorContextImpl(newProperties, _correlationHandler), cancellationToken);

            activeConnectorsByExecutionId.TryAdd(newProperties.GetCorrelationPointId(), executable);

            activeConnectorsByExecutionId.AddOrUpdate(
               newProperties.GetCorrelationPointId(),
               executable,
               (oldkey, oldvalue) => executable);

            ActiveConnectorsByBpmnId.AddOrUpdate(
                newProperties.BpmnProcessId,
                new HashSet<InboundConnectorProperties>() { newProperties },
                (oldkey, oldvalue) =>
                {
                    oldvalue.Add(newProperties);
                    return oldvalue;
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to activate inbound connector Type:{Type}, CorrelationPoint:{CorrelationPoint}", newProperties.Type, newProperties.CorrelationPoint);
        }
    }

    private async Task HandleLatestBpmnVersion(string bpmnId, InboundConnectorProperties[] connectors)
    {
        var alreadyActiveConnectors = ActiveConnectorsByBpmnId.Where(x=> x.Key == bpmnId);
        if (alreadyActiveConnectors?.Any() is true)
        {
            foreach (var item in alreadyActiveConnectors)
            {
                //alreadyActiveConnectors.forEach(this::deactivateConnector);
            }
        }

        foreach (var connector in connectors)
        {
            await ActivateConnector(connector, CancellationToken.None);
        }
    }

    //private void deactivateConnector(InboundConnectorProperties properties)
    //{

    //    InboundConnectorExecutable executable = activeConnectorsByExecutionId.get(properties.getExecutionId());
    //    if (executable == null)
    //    {
    //        throw new IllegalStateException("Connector executable not found for properties " + properties);
    //    }
    //    try
    //    {
    //        executable.deactivate();
    //        activeConnectorsByExecutionId.remove(properties.getExecutionId());
    //        activeConnectorsByBpmnId.get(properties.getBpmnProcessId()).remove(properties);
    //    }
    //    catch (Exception e)
    //    {
    //        // log and continue with other connectors anyway
    //        LOG.error("Failed to deactivate inbound connector " + properties, e);
    //    }
    //}
}
