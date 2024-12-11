using System.ComponentModel;
using Origam.Gui;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public abstract class LabeledEditor(ControlSetItem controlSetItem)
    : BaseIControlAdapter(controlSetItem)
{
    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    public int CaptionLength { get; set; }
    
    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; }
    
    [Localizable(true)]
    [DefaultValue(CaptionPosition.Left)]
    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }
    
    [Browsable(false)]
    [DefaultValue(400)]
    public int Width { get; set; }

    [Browsable(false)]
    [DefaultValue(20)]
    public int Height { get; set; }
    
    [Browsable(false)]
    public int Left { get; set; }

    [Browsable(false)]
    public int Top { get; set; }
}