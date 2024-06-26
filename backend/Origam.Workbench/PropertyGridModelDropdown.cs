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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Origam.Schema;
using System.Windows.Forms.Design;
using System.Globalization;
using System.Linq;

namespace Origam.Workbench;
public partial class PropertyGridModelDropdown : UserControl
{
    IDictionary<string, ISchemaItem> _list = new Dictionary<string, ISchemaItem>();
    IWindowsFormsEditorService _service;
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    bool _isStringList = false;
    public PropertyGridModelDropdown(ISchemaItem value,
        IWindowsFormsEditorService service, ITypeDescriptorContext context)
    {
        InitializeComponent();
        _service = service;
        listBox1.SmallImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
        IEnumerable standardValues = 
            context.PropertyDescriptor.Converter.GetStandardValues(context)
            ?? new TypeConverter.StandardValuesCollection(new ArrayList());
        
        foreach (object item in standardValues)
        {
            if (item is string)
            {
                _isStringList = true;
            }
            ISchemaItem schemaItem = item as ISchemaItem;
            string key = GetKey(item);
            try
            {
                _list.Add(key, schemaItem);
            }
            catch (System.ArgumentException e)
            {
                throw new OrigamException(String.Format("Error while adding key to a dropdown '{0}': {1}",
                    key, e.Message), e);
            }
        }
        Populate();
        if (value != null)
        {
            this.SelectedValue = value;
            ListViewItem selectedItem = this.listBox1.Items[GetKey(value)];
            if (selectedItem != null)
            {
                SelectItem(selectedItem);
            }
        }
    }
    private string GetKey(object item)
    {
        ISchemaItem schemaItem = item as ISchemaItem;
        string key = "";
        if (!_isStringList && schemaItem != null)
        {
            key = schemaItem.PrimaryKey["Id"].ToString();
        }
        else if (item != null)
        {
            key = item.ToString();
        }
        return key;
    }
    private static void SelectItem(ListViewItem selectedItem)
    {
        selectedItem.Selected = true;
        selectedItem.EnsureVisible();
        selectedItem.Focused = true;
    }
    private void Populate()
    {
        string filter = textBox1.Text;
        bool doFilter = string.IsNullOrEmpty(filter);
        listBox1.BeginUpdate();
        listBox1.Items.Clear();
        foreach (var item in _list)
        {
            string value = item.Value != null ? item.Value.ToString() : item.Key;
            if (value != null && (doFilter ||
                CultureInfo.CurrentUICulture.CompareInfo.IndexOf(
                   value, filter, CompareOptions.IgnoreCase) >= 0))
            {
                int icon = -1;
                if (item.Value != null)
                {
                    icon = _schemaBrowser.ImageIndex(item.Value.Icon);
                }
                ListViewItem lvi = listBox1.Items.Add(item.Key, value, icon);
                // in case we are displaying a string list we will return a string
                // instead of the model element
                lvi.Tag = item.Value;
                if (item.Value == null)
                {
                    lvi.Tag = item.Key;
                }
            }
        }
        listBox1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        listBox1.EndUpdate();
    }
    public object SelectedValue { get; set; }
    private void listBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Finish();
        }
        else if (e.KeyCode == Keys.Up && listBox1.SelectedIndices.Count == 0)
        {
            textBox1.Focus();
        }
    }
    private void Finish()
    {
        if (listBox1.SelectedItems.Count == 1)
        {
            this.SelectedValue = listBox1.SelectedItems[0].Tag;
        }
    }
    private void textBox1_TextChanged(object sender, EventArgs e)
    {
        Populate();
        if (listBox1.SelectedIndices.Count == 0 && listBox1.Items.Count > 0)
        {
            listBox1.SelectedItems.Clear();
            SelectItem(listBox1.Items[0]);
        }
    }
    private void textBox1_KeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.Down:
                listBox1.Focus();
                break;
            case Keys.Enter:
                Finish();
                break;
        }
    }
    private void listBox1_ItemActivate(object sender, EventArgs e)
    {
        Finish();
        _service.CloseDropDown();
    }
    private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
    {
        textBox1.Focus();
        SendKeys.Send(e.KeyChar.ToString());
        textBox1.SelectionStart = 1;
        textBox1.SelectionLength = 0;
    }
}
