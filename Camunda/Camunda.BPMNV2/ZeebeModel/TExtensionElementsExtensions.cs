using Spec.BPMN.MODEL;
using System.Xml;

namespace Camunda.BPMNV2.ZeebeModel
{
    public static class TExtensionElementsExtensions
    {
        public static ZeebeProperty[] GetZeebeProperties(this TExtensionElements extensionElements)
        {
            return extensionElements.Any.FirstOrDefault(x => x.LocalName == "properties")
                ?.ChildNodes.Cast<XmlElement>()
                .Select(x => new ZeebeProperty(x.Attributes["name"].Value, x.Attributes["value"].Value))
                .ToArray() ?? Array.Empty<ZeebeProperty>();
        }
    }
}
