#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class ChartSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public ChartSchemaItemProvider() { }
		
		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return AbstractChart.ItemTypeConst;
			}
		}
		public override string Group
		{
			get
			{
				return "UI";
			}
		}
		#endregion

		public ArrayList Charts(Guid formId, string entity)
		{
			ArrayList result = new ArrayList();

			foreach(AbstractChart chart in this.ChildItems)
			{
				foreach(ChartFormMapping m in chart.ChildItemsByType(ChartFormMapping.ItemTypeConst))
				{
					if(formId.Equals(m.Screen.Id) && entity.Equals(m.Entity.Name))
					{
						result.Add(chart);
						break;
					}
				}
			}

			return result;
		}

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "75";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Charts";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
				return "List of Charts";
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {typeof(CartesianChart), 
									  typeof(PieChart),
									typeof(SvgChart)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(CartesianChart))
			{
				item =  new CartesianChart(schemaExtensionId);
				item.Name = "NewCartesianChart";
			}
			else if(type == typeof(PieChart))
			{
				item =  new PieChart(schemaExtensionId);
				item.Name = "NewPieChart";
			}
			else if(type == typeof(SvgChart))
			{
				item =  new SvgChart(schemaExtensionId);
				item.Name = "NewSvgChart";
			}
			else
			{
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorGraphicsUnknownType"));
			}

			item.RootProvider = this;
			item.PersistenceProvider = this.PersistenceProvider;
			item.Group = group;
			this.ChildItems.Add(item);

			return item;
		}

		#endregion


	}
}
