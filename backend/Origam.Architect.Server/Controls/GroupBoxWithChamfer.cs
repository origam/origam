using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public abstract class GroupBoxWithChamfer: ControlBase
{
    [Localizable(true)]
    public string Text { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }
}
