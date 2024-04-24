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
using Origam.Services;
using Origam.Workbench.Services;

namespace Origam.Schema.TestModel
{
	public class TestChecklistRuleConverter : System.ComponentModel.TypeConverter
	{
		static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

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
			TestChecklistRuleSchemaItemProvider rules = _schema.GetProvider(typeof(TestChecklistRuleSchemaItemProvider)) as TestChecklistRuleSchemaItemProvider;

			ArrayList osArray = new ArrayList(rules.ChildItems.Count);
			foreach(AbstractSchemaItem os in rules.ChildItems)
			{
				osArray.Add(os);
			}

			osArray.Sort();

			return new StandardValuesCollection(osArray);
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
				TestChecklistRuleSchemaItemProvider rules = _schema.GetProvider(typeof(TestChecklistRuleSchemaItemProvider)) as TestChecklistRuleSchemaItemProvider;

				foreach(AbstractSchemaItem item in rules.ChildItems)
				{
					if(item.Name == value.ToString())
						return item as TestChecklistRule;
				}
				return null;
			}
			else
				return base.ConvertFrom(context, culture, value);
		}
	}
}
