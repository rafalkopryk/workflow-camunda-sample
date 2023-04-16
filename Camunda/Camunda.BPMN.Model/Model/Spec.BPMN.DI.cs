//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// This code was generated by XmlSchemaClassGenerator version 2.0.805.0 using the following command:
// xscgen BPMN20.xsd -o ..\Model -s --nc
namespace Spec.BPMN.DI;

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("BPMNDiagram", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute("BPMNDiagram", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public partial class BPMNDiagram : Spec.DD.DI.Diagram
{
    
    [System.ComponentModel.DataAnnotations.RequiredAttribute()]
    [System.Xml.Serialization.XmlElementAttribute("BPMNPlane")]
    public BPMNPlane BPMNPlane { get; set; }
    
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    private System.Collections.ObjectModel.Collection<BPMNLabelStyle> _bPMNLabelStyle;
    
    [System.Xml.Serialization.XmlElementAttribute("BPMNLabelStyle")]
    public System.Collections.ObjectModel.Collection<BPMNLabelStyle> BPMNLabelStyle
    {
        get
        {
            return _bPMNLabelStyle;
        }
        private set
        {
            _bPMNLabelStyle = value;
        }
    }
    
    /// <summary>
    /// <para xml:lang="en">Gets a value indicating whether the BPMNLabelStyle collection is empty.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool BPMNLabelStyleSpecified
    {
        get
        {
            return (this.BPMNLabelStyle.Count != 0);
        }
    }
    
    /// <summary>
    /// <para xml:lang="en">Initializes a new instance of the <see cref="BPMNDiagram" /> class.</para>
    /// </summary>
    public BPMNDiagram()
    {
        this._bPMNLabelStyle = new System.Collections.ObjectModel.Collection<BPMNLabelStyle>();
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("BPMNPlane", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute("BPMNPlane", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public partial class BPMNPlane : Spec.DD.DI.Plane
{
    
    [System.Xml.Serialization.XmlAttributeAttribute("bpmnElement")]
    public System.Xml.XmlQualifiedName BpmnElement { get; set; }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("BPMNLabelStyle", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute("BPMNLabelStyle", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public partial class BPMNLabelStyle : Spec.DD.DI.Style
{
    
    [System.ComponentModel.DataAnnotations.RequiredAttribute()]
    [System.Xml.Serialization.XmlElementAttribute("Font", Namespace="http://www.omg.org/spec/DD/20100524/DC")]
    public Spec.DD.DC.Font Font { get; set; }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("BPMNEdge", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute("BPMNEdge", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public partial class BPMNEdge : Spec.DD.DI.LabeledEdge
{
    
    [System.Xml.Serialization.XmlElementAttribute("BPMNLabel")]
    public BPMNLabel BPMNLabel { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("bpmnElement")]
    public System.Xml.XmlQualifiedName BpmnElement { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("sourceElement")]
    public System.Xml.XmlQualifiedName SourceElement { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("targetElement")]
    public System.Xml.XmlQualifiedName TargetElement { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("messageVisibleKind")]
    public MessageVisibleKind MessageVisibleKind { get; set; }
    
    /// <summary>
    /// <para xml:lang="en">Gets or sets a value indicating whether the MessageVisibleKind property is specified.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool MessageVisibleKindSpecified { get; set; }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("BPMNLabel", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute("BPMNLabel", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public partial class BPMNLabel : Spec.DD.DI.Label
{
    
    [System.Xml.Serialization.XmlAttributeAttribute("labelStyle")]
    public System.Xml.XmlQualifiedName LabelStyle { get; set; }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("MessageVisibleKind", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public enum MessageVisibleKind
{
    
    [System.Xml.Serialization.XmlEnumAttribute("initiating")]
    Initiating,
    
    [System.Xml.Serialization.XmlEnumAttribute("non_initiating")]
    Non_Initiating,
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("BPMNShape", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlRootAttribute("BPMNShape", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public partial class BPMNShape : Spec.DD.DI.LabeledShape
{
    
    [System.Xml.Serialization.XmlElementAttribute("BPMNLabel")]
    public BPMNLabel BPMNLabel { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("bpmnElement")]
    public System.Xml.XmlQualifiedName BpmnElement { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("isHorizontal")]
    public bool IsHorizontal { get; set; }
    
    /// <summary>
    /// <para xml:lang="en">Gets or sets a value indicating whether the IsHorizontal property is specified.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool IsHorizontalSpecified { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("isExpanded")]
    public bool IsExpanded { get; set; }
    
    /// <summary>
    /// <para xml:lang="en">Gets or sets a value indicating whether the IsExpanded property is specified.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool IsExpandedSpecified { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("isMarkerVisible")]
    public bool IsMarkerVisible { get; set; }
    
    /// <summary>
    /// <para xml:lang="en">Gets or sets a value indicating whether the IsMarkerVisible property is specified.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool IsMarkerVisibleSpecified { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("isMessageVisible")]
    public bool IsMessageVisible { get; set; }
    
    /// <summary>
    /// <para xml:lang="en">Gets or sets a value indicating whether the IsMessageVisible property is specified.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool IsMessageVisibleSpecified { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("participantBandKind")]
    public ParticipantBandKind ParticipantBandKind { get; set; }
    
    /// <summary>
    /// <para xml:lang="en">Gets or sets a value indicating whether the ParticipantBandKind property is specified.</para>
    /// </summary>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool ParticipantBandKindSpecified { get; set; }
    
    [System.Xml.Serialization.XmlAttributeAttribute("choreographyActivityShape")]
    public System.Xml.XmlQualifiedName ChoreographyActivityShape { get; set; }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.805.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("ParticipantBandKind", Namespace="http://www.omg.org/spec/BPMN/20100524/DI")]
public enum ParticipantBandKind
{
    
    [System.Xml.Serialization.XmlEnumAttribute("top_initiating")]
    Top_Initiating,
    
    [System.Xml.Serialization.XmlEnumAttribute("middle_initiating")]
    Middle_Initiating,
    
    [System.Xml.Serialization.XmlEnumAttribute("bottom_initiating")]
    Bottom_Initiating,
    
    [System.Xml.Serialization.XmlEnumAttribute("top_non_initiating")]
    Top_Non_Initiating,
    
    [System.Xml.Serialization.XmlEnumAttribute("middle_non_initiating")]
    Middle_Non_Initiating,
    
    [System.Xml.Serialization.XmlEnumAttribute("bottom_non_initiating")]
    Bottom_Non_Initiating,
}
