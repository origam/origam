using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public abstract class ControlBase
{
    [Category("Layout")]
    [Browsable(false)]
    public int Height { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Top { get; set; }
    
    [Category("Layout")]
    [Browsable(false)]
    public int Left { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public int Width { get; set; }
}