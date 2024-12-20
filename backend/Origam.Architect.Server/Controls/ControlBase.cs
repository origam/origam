using System.ComponentModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public abstract class ControlBase: IControl
{
    [ReferenceProperty("StyleId")]
    [TypeConverter(typeof(StylesConverter))]
    public Guid StyleId { get; set; }
    
    [Category("Layout")]
    [Browsable(false)]
    public int Top { get; set; }
    
    [Category("Layout")]
    [Browsable(false)]
    public int Left { get; set; }

    [Category("Layout")]
    [Browsable(false)]
    public virtual int Height { get; set; } = 200;

    [Category("Layout")]
    [Browsable(false)]
    public virtual int Width { get; set; } = 200;

    public virtual void Initialize(ControlSetItem controlSetItem)
    {
    }
}