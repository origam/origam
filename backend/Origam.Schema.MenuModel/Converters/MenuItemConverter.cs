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

using System.Collections.Generic;
using System.ComponentModel;
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel;

public class MenuItemConverter : TypeConverter
{
    ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list,
        //but allow free-form entry
        return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        MenuSchemaItemProvider menu =
            _schema.GetProvider(typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
        var menuArray = new List<string>();
        foreach (ISchemaItem item in menu.ChildItemsRecursive)
        {
            if (item is AbstractMenuItem && !(item is Menu || item is Submenu))
            {
                menuArray.Add(item.Path);
            }
        }
        menuArray.Add(null);
        menuArray.Sort();
        return new StandardValuesCollection(menuArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            MenuSchemaItemProvider menu =
                _schema.GetProvider(typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
            foreach (ISchemaItem item in menu.ChildItemsRecursive)
            {
                if (item.Path == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
