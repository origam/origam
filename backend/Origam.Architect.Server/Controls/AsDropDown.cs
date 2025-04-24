using System.ComponentModel;
using Origam.Architect.Server.Attributes;

namespace Origam.Architect.Server.Controls;

public class AsDropDown: LabeledEditor, IAsControl
{
    public bool HideOnForm { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Browsable(false)]
    public Guid LookupId { get; set; }

    public bool ShowUniqueValues { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    public bool ReadOnly { get; set; }
    
    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public Object LookupValue { get; set; }
    
    [NotAModelProperty]
    public string DefaultBindableProperty => "LookupValue";
}
