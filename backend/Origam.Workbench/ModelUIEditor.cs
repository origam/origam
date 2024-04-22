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

using Origam.Schema;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms.Design;

namespace Origam.Workbench;

public class ModelUIEditor : UITypeEditor
{
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
            return UITypeEditorEditStyle.DropDown;
        }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
            IWindowsFormsEditorService editorService = null;
            if (provider != null)
            {
                editorService =
                    provider.GetService(
                    typeof(IWindowsFormsEditorService))
                    as IWindowsFormsEditorService;
            }
            if (editorService != null)
            {
                PropertyGridModelDropdown selectionControl =
                    new PropertyGridModelDropdown(
                    (ISchemaItem)value,
                    editorService, context);

                editorService.DropDownControl(selectionControl);
                if (selectionControl.SelectedValue == null)
                {
                    return null;
                }
                else if(selectionControl.SelectedValue is ISchemaItem)
                {
                    value = selectionControl.SelectedValue;
                }
                else
                {
                    TypeConverter converter = context.PropertyDescriptor.Converter;
                    value = converter.ConvertFrom(context, CultureInfo.CurrentUICulture, selectionControl.SelectedValue);
                }
            }
            return value;
        }

    public override bool GetPaintValueSupported(ITypeDescriptorContext context)
    {
            return true;
        }

    public override bool IsDropDownResizable
    {
        get
        {
                return true;
            }
    }

    public override void PaintValue(PaintValueEventArgs e)
    {
            ISchemaItem schemaItem = e.Value as ISchemaItem;
            if (schemaItem == null)
            {
                System.Diagnostics.Debug.WriteLine("Default paint");
                e.Graphics.ExcludeClip(e.Bounds);
                base.PaintValue(e);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Overriden paint");
                var schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as IBrowserPad;
                var imageList = schemaBrowser.ImageList;
                int icon = schemaBrowser.ImageIndex(schemaItem.Icon);
                e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 1));
                e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Y, 1, e.Bounds.Height));
                e.Graphics.ExcludeClip(new Rectangle(e.Bounds.Width, e.Bounds.Y, 1, e.Bounds.Height));
                e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Height, e.Bounds.Width, 1));
                e.Graphics.DrawImage(imageList.Images[icon], e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height);
            }
        }
}