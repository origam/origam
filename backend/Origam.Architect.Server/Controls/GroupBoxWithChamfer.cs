using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public abstract class GroupBoxWithChamfer
{
    [Browsable(false)]
    public int Height { get; set; }

    [Browsable(false)]
    public int Top { get; set; }

    [Localizable(true)]
    public string Text { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

}
