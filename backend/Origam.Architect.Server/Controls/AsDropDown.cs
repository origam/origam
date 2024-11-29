using System.ComponentModel;
using Origam.Gui;

namespace Origam.Architect.Server.Controls;

public class AsDropDown
{
    public bool HideOnForm { get; set; }

    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; }

    [Browsable(false)]
    public int Height { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Browsable(false)]
    public Guid LookupId { get; set; }

    public bool ShowUniqueValues { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Browsable(false)]
    public int Top { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    [Browsable(false)]
    public Guid StyleId { get; set; }

    public bool ReadOnly { get; set; }

    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public Object LookupValue { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; }

}
