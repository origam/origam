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
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms.Design;
using Origam.Schema;

namespace Origam.Workbench;

public class ModelUIEditor : UITypeEditor
{
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
        return UITypeEditorEditStyle.DropDown;
    }

    public override object EditValue(
        ITypeDescriptorContext context,
        IServiceProvider provider,
        object value
    )
    {
        IWindowsFormsEditorService editorService = null;
        if (provider != null)
        {
            editorService =
                provider.GetService(serviceType: typeof(IWindowsFormsEditorService))
                as IWindowsFormsEditorService;
        }
        if (editorService != null)
        {
            PropertyGridModelDropdown selectionControl = new PropertyGridModelDropdown(
                value: (ISchemaItem)value,
                service: editorService,
                context: context
            );
            editorService.DropDownControl(control: selectionControl);
            if (selectionControl.SelectedValue == null)
            {
                return null;
            }

            if (selectionControl.SelectedValue is ISchemaItem)
            {
                value = selectionControl.SelectedValue;
            }
            else
            {
                TypeConverter converter = context.PropertyDescriptor.Converter;
                value = converter.ConvertFrom(
                    context: context,
                    culture: CultureInfo.CurrentUICulture,
                    value: selectionControl.SelectedValue
                );
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
        get { return true; }
    }

    public override void PaintValue(PaintValueEventArgs e)
    {
        ISchemaItem schemaItem = e.Value as ISchemaItem;
        if (schemaItem == null)
        {
            System.Diagnostics.Debug.WriteLine(message: "Default paint");
            e.Graphics.ExcludeClip(rect: e.Bounds);
            base.PaintValue(e: e);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(message: "Overriden paint");
            var schemaBrowser =
                WorkbenchSingleton.Workbench.GetPad(type: typeof(IBrowserPad)) as IBrowserPad;
            var imageList = schemaBrowser.ImageList;
            int icon = schemaBrowser.ImageIndex(icon: schemaItem.Icon);
            e.Graphics.ExcludeClip(
                rect: new Rectangle(x: e.Bounds.X, y: e.Bounds.Y, width: e.Bounds.Width, height: 1)
            );
            e.Graphics.ExcludeClip(
                rect: new Rectangle(x: e.Bounds.X, y: e.Bounds.Y, width: 1, height: e.Bounds.Height)
            );
            e.Graphics.ExcludeClip(
                rect: new Rectangle(
                    x: e.Bounds.Width,
                    y: e.Bounds.Y,
                    width: 1,
                    height: e.Bounds.Height
                )
            );
            e.Graphics.ExcludeClip(
                rect: new Rectangle(
                    x: e.Bounds.X,
                    y: e.Bounds.Height,
                    width: e.Bounds.Width,
                    height: 1
                )
            );
            e.Graphics.DrawImage(
                image: imageList.Images[index: icon],
                x: e.Bounds.X + 6,
                y: e.Bounds.Y,
                width: e.Bounds.Height,
                height: e.Bounds.Height
            );
        }
    }
}
