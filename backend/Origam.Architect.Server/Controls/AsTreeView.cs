using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class AsTreeView: ControlBase
{
    [Category("Data")]
    [Description("Identifier of the parent. Note: this member must have the same type as identifier column.")]
    public string ParentIDColumn { get; set; }
    
    [Category("Data")]
    [Description("Identifier member, in most cases this is primary column of the table.")]
    public string IDColumn { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }
    
    [Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [Category("Data")]
    [Description("Data member of the tree.")]
    public string DataMember { get; set; }
    
    [Category("Data")]
    [Description("Name member. Note: editing of this column available only with types that support converting from string.")]
    public string NameColumn { get; set; }
}
