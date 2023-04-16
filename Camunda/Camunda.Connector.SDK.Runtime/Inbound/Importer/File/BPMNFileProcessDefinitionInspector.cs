using Camunda.Connector.SDK.Core.Impl.Inbound;
using Spec.BPMN.MODEL;
using System.Xml.Serialization;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer.File;

internal class BPMNFileProcessDefinitionInspector : IProcessDefinitionInspector
{
    private readonly IBpmnProvider _bpmnProvider;

    public BPMNFileProcessDefinitionInspector(IBpmnProvider bpmnProvider)
    {
        _bpmnProvider = bpmnProvider;
    }

    public async Task<InboundConnectorProperties[]> FindInboundConnectors(ProcessDefinition processDefinition)
    {
        byte[] bytes = await _bpmnProvider.GetBpmn(processDefinition);
        using Stream stream = new MemoryStream(bytes);
        stream.Seek(0, SeekOrigin.Begin);
        var serializer = new XmlSerializer(typeof(TDefinitions));
        var definitions = (TDefinitions)serializer.Deserialize(stream);

        return definitions.GetInboundConnectorProperties();
    }
}
