using Spec.BPMN.MODEL;
using System.Xml;

namespace Camunda.BPMN.Model.ZeebeModel
{
    public static class TExtensionElementsExtensions
    {
        public static ZeebeProperty[] GetZeebeProperties(this TExtensionElements extensionElements)
        {
            return extensionElements?.Any.FirstOrDefault(x => x.LocalName == "properties")
                ?.ChildNodes.Cast<XmlElement>()
                .Select(x => new ZeebeProperty(x.GetAttribute("name"), x.GetAttribute("value")))
                .ToArray() ?? Array.Empty<ZeebeProperty>();
        }
    }
}
