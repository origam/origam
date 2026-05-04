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
using Origam.Schema.EntityModel;

namespace Origam.Schema.LookupModel;

public class DataServiceDataLookupListMethodConverter : System.ComponentModel.TypeConverter
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
        DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

        if (reference.ListDataStructure == null)
        {
            return null;
        }

        List<DataStructureMethod> methods =
            reference.ListDataStructure.ChildItemsByType<DataStructureMethod>(
                itemType: DataStructureMethod.CategoryConst
            );
        var methodArray = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            methodArray.Add(item: method);
        }
        methodArray.Add(item: null);
        methodArray.Sort();
        return new StandardValuesCollection(values: methodArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

            if (reference.ListDataStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods =
                reference.ListDataStructure.ChildItemsByType<DataStructureMethod>(
                    itemType: DataStructureMethod.CategoryConst
                );
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataServiceDataLookupValueFilterConverter : System.ComponentModel.TypeConverter
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
        DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

        if (reference.ValueDataStructure == null)
        {
            return null;
        }

        List<DataStructureMethod> methods = reference.ValueDataStructure.Methods;
        var methodArray = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            methodArray.Add(item: method);
        }
        methodArray.Add(item: null);
        methodArray.Sort();
        return new StandardValuesCollection(values: methodArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

            if (reference.ValueDataStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = reference.ValueDataStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataServiceDataTooltipFilterConverter : System.ComponentModel.TypeConverter
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
        AbstractDataTooltip reference = context.Instance as AbstractDataTooltip;

        if (reference.TooltipDataStructure == null)
        {
            return null;
        }

        List<DataStructureMethod> methods = reference.TooltipDataStructure.Methods;
        var methodArray = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            methodArray.Add(item: method);
        }
        methodArray.Add(item: null);
        methodArray.Sort();
        return new StandardValuesCollection(values: methodArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            AbstractDataTooltip reference = context.Instance as AbstractDataTooltip;

            if (reference.TooltipDataStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = reference.TooltipDataStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataServiceDataLookupValueSortSetConverter : System.ComponentModel.TypeConverter
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
        DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

        if (reference.ValueDataStructure == null)
        {
            return null;
        }

        List<DataStructureSortSet> sortSets = reference.ValueDataStructure.SortSets;
        var sortSetArray = new List<DataStructureSortSet>(capacity: sortSets.Count);
        foreach (DataStructureSortSet sortSet in sortSets)
        {
            sortSetArray.Add(item: sortSet);
        }
        sortSetArray.Add(item: null);
        sortSetArray.Sort();
        return new StandardValuesCollection(values: sortSetArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

            if (reference.ValueDataStructure == null)
            {
                return null;
            }

            List<DataStructureSortSet> sortSets = reference.ValueDataStructure.SortSets;
            foreach (DataStructureSortSet item in sortSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class DataServiceDataLookupListSortSetConverter : System.ComponentModel.TypeConverter
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
        DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

        if (reference.ListDataStructure == null)
        {
            return null;
        }

        List<DataStructureSortSet> sortSets = reference.ListDataStructure.SortSets;
        var sortSetArray = new List<DataStructureSortSet>(capacity: sortSets.Count);
        foreach (DataStructureSortSet sortSet in sortSets)
        {
            sortSetArray.Add(item: sortSet);
        }
        sortSetArray.Add(item: null);
        sortSetArray.Sort();
        return new StandardValuesCollection(values: sortSetArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;

            if (reference.ListDataStructure == null)
            {
                return null;
            }

            List<DataStructureSortSet> sortSets = reference.ListDataStructure.SortSets;
            foreach (DataStructureSortSet item in sortSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}
