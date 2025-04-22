using System.ComponentModel;
using Origam.Gui;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public abstract class LabeledEditor: ControlBase
{
    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; } = 100;

    [Category("(ORIGAM)")]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; } = 100;

    [Localizable(true)]
    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; } = CaptionPosition.Left;
    
    public override void Initialize(ControlSetItem controlSetItem)
    {
        Height = 20;
        Width = 400;
    }
}