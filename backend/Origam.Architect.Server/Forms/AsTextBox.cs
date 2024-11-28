using Origam.Gui;
using System.ComponentModel;

namespace Origam.Architect.Server.Forms;

public class AsTextBox
{
    [Browsable(false)]
    public Guid StyleId { get; set; }

    [Category("(ORIGAM)")]
    public int CaptionLength { get; set; }

    [Category("(ORIGAM)")]
    [DefaultValue(100)]
    [Description("Column Width (in pixels) to be used in grid-view. If the value is less than then zero, then the column is hidden by default. However, when it's enabled, the abs(configured value) is used.")]
    public int GridColumnWidth { get; set; }

    [Description("Valid only for numeric data types. If specified, it will override default formatting for the given data type.")]
    public string CustomNumericFormat { get; set; }

    [DefaultValue(false)]
    public bool AllowTab { get; set; }

    [DefaultValue(false)]
    public bool ReadOnly { get; set; }
    
    [Localizable(true)]
    [DefaultValue(CaptionPosition.None)]
    [Category("(ORIGAM)")]
    public CaptionPosition CaptionPosition { get; set; }

    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public bool HideOnForm { get; set; }

    [DefaultValue(false)]
    public bool IsRichText { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    [Browsable(false)]
    public int Top { get; set; }

    public bool Multiline { get; set; }

    [Browsable(false)]
    public int Height { get; set; }

    public bool IsPassword { get; set; }

    public Object Value { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

}
