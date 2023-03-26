using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Spec.BPMN.MODEL;
using Camunda.BPMNV2.ZeebeModel;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer.File;

internal static class BpmnDefintitionsExtensions
{
    private const string CorrelationKeyMappingKey = "inbound.correlationKeyMapping";

    private static T[] GetElements<T>(this IEnumerable<TRootElement> rootElements)
    {
        return rootElements.Where(x => x is T).Cast<T>().ToArray() ?? Array.Empty<T>();
    }

    public static InboundConnectorProperties[] GetInboundConnectorProperties(this TDefinitions definitions)
    {
        var elements = definitions.RootElement;
        var messages = elements.GetElements<TMessage>();
        var processes = elements.GetElements<TProcess>();

        return processes.Select(x => x.FlowElement.GetInboundConnectorProperties(x, messages)).SelectMany(x => x).ToArray();
    }

    public static InboundConnectorProperties[] GetInboundConnectorProperties(this IEnumerable<TFlowElement> flowElements, TProcess bpmnProcess, TMessage[] messages)
    {
        return flowElements.Select(x => x switch
        {
            TReceiveTask receiveTask => receiveTask.GetInboundConnectorProperties(bpmnProcess, messages),
            TIntermediateCatchEvent intermediateCatchEvent => GetInboundConnectorProperties(intermediateCatchEvent, bpmnProcess, messages),
            TServiceTask serviceTask => serviceTask.GetInboundConnectorProperties(bpmnProcess),
            TSubProcess subProcess => subProcess.FlowElement.GetInboundConnectorProperties(bpmnProcess, messages),
            _ => Array.Empty<InboundConnectorProperties>(),
        }).SelectMany(x => x).ToArray();
    }

    private static InboundConnectorProperties[] GetInboundConnectorProperties(TIntermediateCatchEvent intermediateCatchEvent, TProcess bpmnProcess, TMessage[] messages)
    {
        var messageEventDefinition = intermediateCatchEvent.EventDefinition.FirstOrDefault(x => x is TMessageEventDefinition) as TMessageEventDefinition;
        if (messageEventDefinition is null)
        {
            return Array.Empty<InboundConnectorProperties>();
        }

        var messageName = messages.FirstOrDefault(x => x.Id == messageEventDefinition.MessageRef?.Name)?.Name;
        if (string.IsNullOrWhiteSpace(messageName))
        {
            return Array.Empty<InboundConnectorProperties>();
        }

        var properties = intermediateCatchEvent?.ExtensionElements.GetZeebeProperties().ToDictionary(x => x.Name, x => x.Value);
        var corellationKeyMapping = properties.FirstOrDefault(x => x.Key == CorrelationKeyMappingKey).Value;
        return new InboundConnectorProperties[]
        {
            new InboundConnectorProperties
            {
                BpmnProcessId = bpmnProcess.Id,
                Properties = properties,
                CorrelationPoint = new MessageCorrelationPoint(messageName, corellationKeyMapping)
            }
        };
    }

    public static InboundConnectorProperties[] GetInboundConnectorProperties(this TReceiveTask element, TProcess bpmnProcess, TMessage[] messages)
    {
        var messageName = messages.FirstOrDefault(x => x.Id == element.MessageRef?.Name)?.Name;
        if (string.IsNullOrWhiteSpace(messageName))
        {
            return Array.Empty<InboundConnectorProperties>();
        }

        var properties = element?.ExtensionElements.GetZeebeProperties().ToDictionary(x => x.Name, x => x.Value);
        var corellationKeyMapping = properties.FirstOrDefault(x => x.Key == CorrelationKeyMappingKey).Value;
        return new InboundConnectorProperties[]
        {
            new InboundConnectorProperties
            {
                BpmnProcessId = bpmnProcess.Id,
                Properties = properties,
                CorrelationPoint = new MessageCorrelationPoint(messageName, corellationKeyMapping)
            }
        };
    }

    public static InboundConnectorProperties[] GetInboundConnectorProperties(this TServiceTask element, TProcess bpmnProcess)
    {
        return Array.Empty<InboundConnectorProperties>();
    }
}
