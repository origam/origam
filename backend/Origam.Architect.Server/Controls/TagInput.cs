using System.ComponentModel;
using Origam.Architect.Server.Attributes;

namespace Origam.Architect.Server.Controls;

public class TagInput: LabeledEditor, IAsControl
{
    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Browsable(false)]
    public Guid LookupId { get; set; }
    
    public bool HideOnForm { get; set; }

    public string Value { get; set; }
    
    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }
    
    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    public bool ReadOnly { get; set; }
    
    [NotAModelProperty]
    public string DefaultBindableProperty => "Value";
}