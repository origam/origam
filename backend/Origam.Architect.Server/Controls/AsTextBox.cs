using System.ComponentModel;
using Origam.Architect.Server.Attributes;
using Origam.Schema.GuiModel;

namespace Origam.Architect.Server.Controls;

public class AsTextBox: LabeledEditor, IAsControl
{
    [Description("Valid only for numeric data types. If specified, it will override default formatting for the given data type.")]
    public string CustomNumericFormat { get; set; }

    public bool AllowTab { get; set; } = false;
    
    [Category("Behavior")]
    public bool ReadOnly { get; set; } = false;
    
    [Category("(ORIGAM)")]
    public string GridColumnCaption { get; set; }

    [Category("(ORIGAM)")]
    public string Caption { get; set; }

    [Localizable(true)]
    [Category("Behavior")]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    public bool HideOnForm { get; set; }
    
    public bool IsRichText { get; set; } = false;
    
    [Category("Behavior")]
    public bool Multiline { get; set; }
    
    public bool IsPassword { get; set; }

    public Object Value { get; set; }
    
    [NotAModelProperty]
    public string DefaultBindableProperty => "Value";
}