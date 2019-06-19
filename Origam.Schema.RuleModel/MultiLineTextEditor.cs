using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Origam.Schema.RuleModel
{
#if !NETSTANDARD
    class MultiLineTextEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _editorService;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            _editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            TextBox textEditorBox = new TextBox();
            textEditorBox.Multiline = true;
            textEditorBox.AcceptsTab = true;
            textEditorBox.WordWrap = true;
            textEditorBox.ScrollBars = ScrollBars.Vertical;
            textEditorBox.Width = 250;
            textEditorBox.Height = 150;
            textEditorBox.BorderStyle = BorderStyle.None;
            textEditorBox.AcceptsReturn = true;
            textEditorBox.Text = value as string;
            _editorService.DropDownControl(textEditorBox);
            return textEditorBox.Text;

        }
    }
#endif
}
