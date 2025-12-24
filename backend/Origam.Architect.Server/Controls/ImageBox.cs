using System.ComponentModel;
using Origam.Gui;

namespace Origam.Architect.Server.Controls;

public class ImageBox : ControlBase
{
    [Category("(ORIGAM)")]
    [Description(
        "Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used."
    )]
    public int GridColumnWidth { get; set; } = 100;

    [Browsable(true)]
    public int TabIndex { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Browsable(true)]
    public ImageBoxSourceType SourceType { get; set; }

    public Object ImageData { get; set; }
}
