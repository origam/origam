using System.ComponentModel;
using Origam.Gui;

namespace Origam.Architect.Server.Controls;

public class AsTextBox: LabeledEditor
{
    [Browsable(false)]
    public Guid StyleId { get; set; }
    
    [Description("Valid only for numeric data types. If specified, it will override default formatting for the given data type.")]
    public string CustomNumericFormat { get; set; }

    [DefaultValue(false)]
    public bool AllowTab { get; set; }

    [DefaultValue(false)]
    public bool ReadOnly { get; set; }
    
    [Localizable(true)]
    [DefaultValue(CaptionPosition.Left)]
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
    
    public bool Multiline { get; set; }
    
    public bool IsPassword { get; set; }

    public Object Value { get; set; }
}
