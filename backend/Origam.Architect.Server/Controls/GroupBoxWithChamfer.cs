using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class GroupBoxWithChamfer: ControlBase
{
    [Localizable(true)]
    public string Text { get; set; } = "Group Box";

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }
}
