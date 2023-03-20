using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Xml.Linq;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer;

public interface IBpmnProvider
{
    byte[] GetBpmn(ProcessDefinition processDefinition);
}

public class BPMNFileProcessDefinitionInspector : IProcessDefinitionInspector
{
    private readonly IBpmnProvider _bpmnProvider;

    public BPMNFileProcessDefinitionInspector(IBpmnProvider bpmnProvider)
    {
        _bpmnProvider = bpmnProvider;
    }

    public InboundConnectorProperties[] FindInboundConnectors(ProcessDefinition processDefinition)
    {
        byte[] bytes = _bpmnProvider.GetBpmn(processDefinition);
        using var stream = new MemoryStream(bytes);
        var document = XDocument.Load(stream);
        var processes = document.Root.Descendants().Where(x => x.Name.LocalName == "process").ToArray();

        var bpmnProcesses = new List<BpmnProcess>();
        foreach (var process in processes)
        {
            var bpmnProcess = BpmnProcess.Build(process);
            bpmnProcesses.Add(bpmnProcess);
            bpmnProcess.ParseChilds();
        }

        return null;
    }
}

public abstract class BpmnElement
{
    public string Id { get; }

    public XElement Element { get; }

    protected BpmnElement(string id, XElement element)
    {
        Id = id;
        Element = element;
    }

    public List<BpmnElement> Elements { get; set; } = new ();

    public abstract void ParseChilds();
}

public class BpmnProcess : BpmnElement
{
    public BpmnProcess(string id, XElement element) : base(id, element)
    {
    }

    public static BpmnProcess Build(XElement element)
    {
        return new BpmnProcess(element.Attribute("id").Value, element);
    }

    public override void ParseChilds()
    {
        var items = Element.Descendants().Where(x => new[] { "serviceTask", "receiveTask" }.Contains(x.Name.LocalName)).ToArray();
        foreach (var item in items)
        {
            var bpmnElement = item.Name.LocalName switch
            {
                "serviceTask" => BpmnServiceTask.Build(item),
                "receiveTask" => (BpmnElement)BpmnReceiveTask.Build(item),
                _ => throw new NotSupportedException($"{item.Name.LocalName} not supported")
            };

            Elements.Add(bpmnElement);
        }
    }
}

public class BpmnServiceTask : BpmnElement
{
    public BpmnExtensionElements ExtensionElements { get; set; }

    public BpmnServiceTask(string id, XElement element, BpmnExtensionElements extensionElements) : base(id, element)
    {
        ExtensionElements = extensionElements;
    }

    public static BpmnServiceTask Build(XElement element)
    {
        return new BpmnServiceTask(element.Attribute("id").Value, element, null);
    }

    public override void ParseChilds()
    {
        
    }
}

public class BpmnReceiveTask : BpmnElement
{
    public BpmnReceiveTask(string id, XElement element, string messageRef, BpmnExtensionElements extensionElements) : base(id, element)
    {
        MessageRef = messageRef;
        ExtensionElements = extensionElements;
    }

    public static BpmnReceiveTask Build(XElement element)
    {
        var zeebeProperties = element.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "extensionElements")
            .Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "properties")
            .Descendants()
            .Where(x => x.Name.LocalName == "property")
            .Select(x => new ZeebeProperty(x.Attribute("name").Value, x.Attribute("value").Value))
            .ToArray();

        var bpmnExtensionElements = new BpmnExtensionElements(zeebeProperties);
        return new BpmnReceiveTask(element.Attribute("id").Value, element, element.Attribute("messageRef").Value, bpmnExtensionElements);
    }

    public string MessageRef { get;  }
    public BpmnExtensionElements ExtensionElements { get; set; }

    public override void ParseChilds()
    {
    }
}

public record ZeebeProperty(string Name, string Value);

public record BpmnExtensionElements(ZeebeProperty[] Properties);

public class MockProcessDefinitionInspector : IProcessDefinitionInspector
{
    private readonly IConfiguration _configuration;

    public MockProcessDefinitionInspector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public InboundConnectorProperties[] FindInboundConnectors(ProcessDefinition processDefinition)
    {

        var inboundConnectorPropertiesOptions = _configuration.GetSection("InboundConnectorProperties").Get<InboundConnectorPropertiesOptions[]>();

        return inboundConnectorPropertiesOptions.Select(x => new InboundConnectorProperties
        {
            BpmnProcessId = x.BpmnProcessId,
            CorrelationPoint = new MessageCorrelationPoint(x.CorrelationPoint.MessageName, x.Properties.FirstOrDefault(x => x.Key == "inbound.correlationKeyMapping").Value),
            Properties = x.Properties
        }).ToArray();
    }
}

public record InboundConnectorPropertiesOptions
{
    public Dictionary<string, string> Properties { get; init; }
    public MessageCorrelationOptions CorrelationPoint { get; init; }
    public string BpmnProcessId { get; init; }
}

public record MessageCorrelationOptions
{
    public string MessageName { get; init; }
}