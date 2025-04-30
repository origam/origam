using System.ComponentModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class AsForm: IControl
{
    [Browsable(false)]
    public string ExtraControlBindings { get; set; }

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