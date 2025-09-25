using Origam.DA.ObjectPersistence;
using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class SectionLevelPlugin__--__c418233f5a214c35a16458fd910fec51: IControl
{
    [Category("Data")]
    public string Path { get; set; }

    [Category("Data")]
    public string MemberDocumentId { get; set; }

    [Browsable(false)]
    public int Width { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    [Browsable(false)]
    public int Height { get; set; }

    [DefaultValue(false)]
    [Category("Data")]
    [Description("Must be set for exactly one plugin per screen to true if there is no master grid present.")]
    public bool AllowNavigation { get; set; }

    [Browsable(false)]
    public int Left { get; set; }

    [Browsable(false)]
    public int Top { get; set; }

    [Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [SettingsBindable(true)]
    public string Text { get; set; }

    [DefaultValue("")]
    [Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [Category("Data")]
    [Description("Data member of the tree.")]
    [NotNullModelElementRule]
    public string DataMember { get; set; }

    public void Initialize(ControlSetItem controlSetItem)
    {
    }
}
