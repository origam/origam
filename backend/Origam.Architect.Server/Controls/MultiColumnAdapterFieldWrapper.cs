using System.ComponentModel;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class MultiColumnAdapterFieldWrapper: IControl
{
    [Browsable(false)]
    public int Top { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

    [Category("Multi Column Adapter Field")]
    public string ControlMember { get; set; }
    
    public int TabIndex { get; set; }

    [Browsable(false)]
    public int Height { get; set; }

    [Browsable(false)]
    public int Left { get; set; }
    
    public void Initialize(ControlSetItem controlSetItem)
    {
    }
}
