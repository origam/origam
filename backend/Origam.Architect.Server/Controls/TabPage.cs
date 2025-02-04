using System.ComponentModel;
using Origam.Architect.Server.Controls;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class TabPage: IControl
{
    [Localizable(true)]
    [Browsable(true)]
    public string Text { get; set; }

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

    public void Initialize(ControlSetItem controlSetItem)
    {
    }
}
