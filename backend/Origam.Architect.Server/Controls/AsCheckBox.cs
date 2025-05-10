using System.ComponentModel;
using Origam.Architect.Server.Attributes;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class AsCheckBox: ControlBase, IAsControl
{
    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    [Category("(ORIGAM)")]
    [Description(
        "Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; } = 100;

    public string Text { get; set; }

    [Browsable(true)]
    [Category("Behavior")]
    public bool ReadOnly { get; set; } = false;

    public Object Value { get; set; }

    public bool HideOnForm { get; set; }
    
    [NotAModelProperty]
    public string DefaultBindableProperty => "Value";

    public override void Initialize(ControlSetItem controlSetItem)
    {
        Text = controlSetItem.Name;
        Height = 20;
    }
}
