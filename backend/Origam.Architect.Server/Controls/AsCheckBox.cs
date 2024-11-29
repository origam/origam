using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class AsCheckBox
{
    [Browsable(false)]
    public int Top { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; }

    public string Text { get; set; }

    [Browsable(true)]
    [Category("Behavior")]
    [DefaultValue(false)]
    public bool ReadOnly { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

    public Object Value { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    public bool HideOnForm { get; set; }

    [Browsable(false)]
    public int Height { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }

}
