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

using Origam.Schema.EntityModel;

namespace Origam.Schema.LookupModel
{
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

		public override System.ComponentModel.TypeConverter.StandardValuesCollection 
			GetStandardValues(ITypeDescriptorContext context)
		{
			DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
			if(reference.ListDataStructure == null) return null;

			ArrayList methods = reference.ListDataStructure.ChildItemsByType(DataStructureMethod.CategoryConst);

			ArrayList methodArray = new ArrayList(methods.Count);
			foreach(DataStructureMethod method in methods)
			{
				methodArray.Add(method);
			}

			methodArray.Add(null);
			methodArray.Sort();

			return new StandardValuesCollection(methodArray);
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
				DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
				if(reference.ListDataStructure == null) return null;

				ArrayList methods = reference.ListDataStructure.ChildItemsByType(DataStructureMethod.CategoryConst);

				foreach(DataStructureMethod item in methods)
				{
					if(item.Name == value.ToString())
						return item as DataStructureMethod;
				}
				return null;
			}
			else
				return base.ConvertFrom(context, culture, value);
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

		public override System.ComponentModel.TypeConverter.StandardValuesCollection 
			GetStandardValues(ITypeDescriptorContext context)
		{
			DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
			if(reference.ValueDataStructure == null) return null;

			ArrayList methods = reference.ValueDataStructure.Methods;

			ArrayList methodArray = new ArrayList(methods.Count);
			foreach(DataStructureMethod method in methods)
			{
				methodArray.Add(method);
			}

			methodArray.Add(null);
			methodArray.Sort();

			return new StandardValuesCollection(methodArray);
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
				DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
				if(reference.ValueDataStructure == null) return null;

				ArrayList methods = reference.ValueDataStructure.Methods;

				foreach(DataStructureMethod item in methods)
				{
					if(item.Name == value.ToString())
						return item as DataStructureMethod;
				}
				return null;
			}
			else
				return base.ConvertFrom(context, culture, value);
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

		public override System.ComponentModel.TypeConverter.StandardValuesCollection 
			GetStandardValues(ITypeDescriptorContext context)
		{
			AbstractDataTooltip reference = context.Instance as AbstractDataTooltip;
			
			if(reference.TooltipDataStructure == null) return null;

			ArrayList methods = reference.TooltipDataStructure.Methods;

			ArrayList methodArray = new ArrayList(methods.Count);
			foreach(DataStructureMethod method in methods)
			{
				methodArray.Add(method);
			}

			methodArray.Add(null);
			methodArray.Sort();

			return new StandardValuesCollection(methodArray);
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
				AbstractDataTooltip reference = context.Instance as AbstractDataTooltip;
			
				if(reference.TooltipDataStructure == null) return null;

				ArrayList methods = reference.TooltipDataStructure.Methods;

				foreach(DataStructureMethod item in methods)
				{
					if(item.Name == value.ToString())
						return item as DataStructureMethod;
				}
				return null;
			}
			else
				return base.ConvertFrom(context, culture, value);
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

		public override System.ComponentModel.TypeConverter.StandardValuesCollection 
			GetStandardValues(ITypeDescriptorContext context)
		{
			DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
			if(reference.ValueDataStructure == null) return null;

			ArrayList sortSets = reference.ValueDataStructure.SortSets;

			ArrayList sortSetArray = new ArrayList(sortSets.Count);
			foreach(DataStructureSortSet sortSet in sortSets)
			{
				sortSetArray.Add(sortSet);
			}

			sortSetArray.Add(null);
			sortSetArray.Sort();

			return new StandardValuesCollection(sortSetArray);
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
				DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
				if(reference.ValueDataStructure == null) return null;

				ArrayList sortSets = reference.ValueDataStructure.SortSets;

				foreach(DataStructureSortSet item in sortSets)
				{
					if(item.Name == value.ToString())
						return item as DataStructureSortSet;
				}
				return null;
			}
			else
				return base.ConvertFrom(context, culture, value);
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

		public override System.ComponentModel.TypeConverter.StandardValuesCollection 
			GetStandardValues(ITypeDescriptorContext context)
		{
			DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
			if(reference.ListDataStructure == null) return null;

			ArrayList sortSets = reference.ListDataStructure.SortSets;

			ArrayList sortSetArray = new ArrayList(sortSets.Count);
			foreach(DataStructureSortSet sortSet in sortSets)
			{
				sortSetArray.Add(sortSet);
			}

			sortSetArray.Add(null);
			sortSetArray.Sort();

			return new StandardValuesCollection(sortSetArray);
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
				DataServiceDataLookup reference = context.Instance as DataServiceDataLookup;
			
				if(reference.ListDataStructure == null) return null;

				ArrayList sortSets = reference.ListDataStructure.SortSets;

				foreach(DataStructureSortSet item in sortSets)
				{
					if(item.Name == value.ToString())
						return item as DataStructureSortSet;
				}
				return null;
			}
			else
				return base.ConvertFrom(context, culture, value);
		}
	}
}
