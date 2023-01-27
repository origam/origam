#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.ComponentModel;
using System.Drawing.Design;
#if !NETSTANDARD
using System.Windows.Forms;
using System.Windows.Forms.Design;
#endif

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
            return textEditorBox.Text == (string)value
                ? value 
                : textEditorBox.Text;
        }
    }
#endif
}
