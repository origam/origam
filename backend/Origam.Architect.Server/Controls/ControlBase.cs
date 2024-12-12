using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public abstract class ControlBase
{
    [Category("Layout")]
    [Browsable(false)]
    public int Top { get; set; }
    
    [Category("Layout")]
    [Browsable(false)]
    public int Left { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    [DefaultValue(200)]
    public virtual int Height { get; set; }
    
    [Category("Layout")]
    [Browsable(false)]
    [DefaultValue(200)]
    public virtual int Width { get; set; }
}