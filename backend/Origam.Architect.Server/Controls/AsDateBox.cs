using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class AsDateBox: LabeledEditor
{
    public bool HideOnForm { get; set; }
    
    public bool ReadOnly { get; set; }

    [DefaultValue("dd.MMMM yyyy")]
    public string CustomFormat { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public Object DateValue { get; set; }
    
    [Browsable(false)]
    public Guid StyleId { get; set; }

}
