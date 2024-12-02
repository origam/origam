using System.ComponentModel;
using Origam.Gui;

namespace Origam.Architect.Server.Controls;

public class AsDateBox
{
    public bool HideOnForm { get; set; }

    [Browsable(true)]
    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; }

    public bool ReadOnly { get; set; }

    [DefaultValue("dd.MMMM yyyy")]
    public string CustomFormat { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public Object DateValue { get; set; }

    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; }

    [Browsable(false)]
    [DefaultValue(400)]
    public int Width { get; set; }

    [Browsable(false)]
    [DefaultValue(20)]
    public int Height { get; set; }

    [Browsable(false)]
    public int Top { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }

}
