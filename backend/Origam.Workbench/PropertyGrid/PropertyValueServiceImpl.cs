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
using System.Drawing.Design;

namespace Origam.Workbench.PropertyGrid;
public sealed class PropertyValueServiceImpl : IPropertyValueUIService
{
    public event EventHandler PropertyUIValueItemsChanged;
    public event PropertyValueUIHandler QueryPropertyUIValueItems;
    public PropertyValueUIItem[]
    GetPropertyUIValueItems(ITypeDescriptorContext context,
    PropertyDescriptor propDesc)
    {
        List<PropertyValueUIItem> list = null;
        if (QueryPropertyUIValueItems != null)
        {
            list = new List<PropertyValueUIItem>();
            QueryPropertyUIValueItems(context, propDesc, list);
        }
        if (list == null || list.Count == 0)
        {
            return new PropertyValueUIItem[0];
        }
        PropertyValueUIItem[] result = new PropertyValueUIItem[list.Count];
        list.CopyTo(result);
        return result;
    }
    public void NotifyPropertyValueUIItemsChanged()
    {
        if (PropertyUIValueItemsChanged != null)
        {
            PropertyUIValueItemsChanged(this, EventArgs.Empty);
        }
    }
    void
    IPropertyValueUIService.RemovePropertyValueUIHandler(PropertyValueUIHandler
    newHandler)
    {
        QueryPropertyUIValueItems -= newHandler;
    }
    void
    IPropertyValueUIService.AddPropertyValueUIHandler(PropertyValueUIHandler
    newHandler)
    {
        QueryPropertyUIValueItems += newHandler;
    }
}
