using Origam.Gui;
using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class ColorPicker: ControlBase
{
    public bool ReadOnly { get; set; }

    [Category("(ORIGAM)")]
    [Description(
        "Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; } = 100;

    public Object SelectedColor { get; set; }
    
    public int TabIndex { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }

    public bool HideOnForm { get; set; }
    
    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; }
}
