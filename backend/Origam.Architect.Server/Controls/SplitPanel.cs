using Origam.Gui;
using System.ComponentModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class SplitPanel: IControl
{
    [Category("Behavior")]
    public int TabIndex { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Top { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Left { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Height { get; set; } = 200;

    [Category("Layout")]
    [Browsable(false)]
    public int Width { get; set; } = 200;

    public bool FixedSize { get; set; }

    public SplitPanelOrientation Orientation { get; set; }

    public void Initialize(ControlSetItem controlSetItem)
    {

    }
}
