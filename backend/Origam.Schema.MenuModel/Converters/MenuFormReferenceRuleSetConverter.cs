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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Origam.Schema.EntityModel;

namespace Origam.Schema.MenuModel;

public class MenuFormReferenceRuleSetConverter : TypeConverter
{
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
        FormReferenceMenuItem currentItem = context.Instance as FormReferenceMenuItem;
        List<DataStructureRuleSet> ruleSets = currentItem.Screen.DataStructure.RuleSets;
        var array = new List<DataStructureRuleSet>(ruleSets.Count);
        foreach (var item in ruleSets)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
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
            FormReferenceMenuItem currentItem = context.Instance as FormReferenceMenuItem;
            List<DataStructureRuleSet> ruleSets = currentItem.Screen.DataStructure.RuleSets;
            foreach (DataStructureRuleSet item in ruleSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
