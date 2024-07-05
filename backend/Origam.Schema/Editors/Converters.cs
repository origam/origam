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

using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace Origam.Schema;
public class ParameterReferenceConverter : System.ComponentModel.TypeConverter
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
		AbstractSchemaItem root = reference.RootItem;
		List<ISchemaItem> parameters = root.Parameters;
		var paramArray = new List<SchemaItemParameter>(parameters.Count);
        paramArray.Add(null);
		foreach(SchemaItemParameter parameter in parameters)
		{
			// TODO: Check for recursion and skip any columns that could cause it
			paramArray.Add(parameter);
		}
		paramArray.Sort();
		return new StandardValuesCollection(paramArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
			AbstractSchemaItem root = reference.RootItem;
			foreach(SchemaItemParameter item in root.Parameters)
			{
				if(item.Name == value.ToString())
					return item as SchemaItemParameter;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class SchemaItemAncestorConverter : System.ComponentModel.TypeConverter
{
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
//		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
//		{
//			if( value.GetType() == typeof(string) )
//			{
////				AbstractSchemaItem reference = context.Instance as AbstractSchemaItem;
////				AbstractSchemaItem root = reference.RootItem;
////
////				ISchemaItemCollection parameters = root.Parameters;
////
////				foreach(SchemaItemParameter item in parameters)
////				{
////					if(item.Name == value.ToString())
////						return item as SchemaItemParameter;
////				}
//				return "Ancestor Items";
//			}
//			else
//				return base.ConvertFrom(context, culture, value);
//		}
}
public class AncestorItemConverter : System.ComponentModel.TypeConverter
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
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		SchemaItemAncestor ancestor = context.Instance as SchemaItemAncestor;
		
		if(ancestor.SchemaItem.ParentItem != null) return new StandardValuesCollection(new ArrayList());
		
		ISchemaItemProvider provider = ancestor.SchemaItem.RootProvider;
		ArrayList items = new ArrayList();
		foreach(ISchemaItem item in provider.ChildItems)
		{
			if(item.IsAbstract && (! item.PrimaryKey.Equals(ancestor.SchemaItem.PrimaryKey)))
			{
				items.Add(item);
			}
		}
		items.Sort();
		return new StandardValuesCollection(items);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			SchemaItemAncestor ancestor = context.Instance as SchemaItemAncestor;
		
			if(ancestor.SchemaItem.ParentItem != null) return new StandardValuesCollection(new ArrayList());
		
			ISchemaItemProvider provider = ancestor.SchemaItem.RootProvider;
			foreach(ISchemaItem item in provider.ChildItems)
			{
				if(item.Name == value.ToString())
					return item;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
