using System.ComponentModel;
using Origam.Gui;

namespace Origam.Architect.Server.Controls;

public abstract class LabeledEditor: ControlBase
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
}