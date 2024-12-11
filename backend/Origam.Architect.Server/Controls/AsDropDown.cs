using System.ComponentModel;
using Origam.Gui;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class AsDropDown(ControlSetItem controlSetItem) : LabeledEditor(controlSetItem)
{
    public bool HideOnForm { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Browsable(false)]
    public Guid LookupId { get; set; }

    public bool ShowUniqueValues { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }

    public bool ReadOnly { get; set; }
    
    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public Object LookupValue { get; set; }
}
