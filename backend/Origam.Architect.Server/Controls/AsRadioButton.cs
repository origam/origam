using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class AsRadioButton : ControlBase
{
    [Browsable(false)]
    public Guid DataConstantId { get; set; }

    public bool ReadOnly { get; set; }

    public string Text { get; set; }

    public int TabIndex { get; set; }

    [Browsable(false)]
    public Object Value { get; set; }
}
