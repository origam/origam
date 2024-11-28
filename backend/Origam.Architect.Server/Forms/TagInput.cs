using Origam.Gui;
using System.ComponentModel;

namespace Origam.Architect.Server.Forms;

public class TagInput
{
    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Browsable(false)]
    public Guid LookupId { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

    [Browsable(false)]
    public int Height { get; set; }

    public bool HideOnForm { get; set; }

    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; }

    public string Value { get; set; }

    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }

    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    [Browsable(false)]
    public int Top { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    public bool ReadOnly { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }

}
