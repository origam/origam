using System.ComponentModel;
using Origam.Architect.Server.Attributes;

namespace Origam.Architect.Server.Controls;

public class AsDateBox: LabeledEditor, IAsControl
{
    public bool HideOnForm { get; set; }
    
    public bool ReadOnly { get; set; }

    public string CustomFormat { get; set; } = "dd.MMMM yyyy";

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public Object DateValue { get; set; }
    
    [NotAModelProperty]
    public string DefaultBindableProperty => "DateValue";
}
