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

using Origam.DA.Common;
using System;
using System.Collections;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel
{
   [SchemaItemDescription("Alternative", "Alternatives", "icon_alternative.png")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class ControlSetItem  : AbstractSchemaItem 
	{

		public const string CategoryConst = "ControlSetItem";

		public ControlSetItem() {}
		
		public ControlSetItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ControlSetItem(Key primaryKey) : base(primaryKey) {}

		#region Properties
		public Guid ControlId;

        [XmlReference("widget", "ControlId")]
		public ControlItem ControlItem
		{
			get => (ControlItem)PersistenceProvider.RetrieveInstance(
				typeof(ControlItem), new ModelElementKey(ControlId));
			set => ControlId = (Guid)value.PrimaryKey["Id"];
		}
		
		private string _roles;
		[XmlAttribute("roles")]
		public string Roles
		{
			get => _roles;
			set => _roles = value;
		}		

		private string _features;
		[XmlAttribute("features")]
		public string Features
		{
			get => _features;
			set => _features = value;
		}		

		private Guid _multiColumnAdapterFieldCondition;
		
        [XmlAttribute("multiColumnAdapterFieldCondition")]
		public Guid MultiColumnAdapterFieldCondition
		{
			get => _multiColumnAdapterFieldCondition;
			set => _multiColumnAdapterFieldCondition = value;
		}

		private bool _isAlternative = false;

        [XmlAttribute("isAlternative")]
        public bool IsAlternative
		{
			get => _isAlternative;
			set => _isAlternative = value;
		}

        private bool _requestSaveAfterChange = false;

        [XmlAttribute("requestSaveAfterChange")]
        public bool RequestSaveAfterChange
        {
            get => _requestSaveAfterChange;
            set => _requestSaveAfterChange = value;
        }

		private int _level = 100;

        [XmlAttribute("level")]
        public int Level
		{
			get => _level;
			set => _level = value;
		}
		#endregion
		
		#region Overriden AbstractSchemaItem Members
		public override string ItemType => CategoryConst;

		public override UI.BrowserNodeCollection ChildNodes()
		{
			// return only the 1st level of items (alternative screen/panels) but not child widgets
			return ParentItem.ParentItem == null 
				? new UI.BrowserNodeCollection() : base.ChildNodes();
		}

		public override void GetExtraDependencies(ArrayList dependencies)
		{
			dependencies.Add(ControlItem);
			if(ControlItem.PanelControlSet != null)
			{
				dependencies.Add(ControlItem.PanelControlSet);
			}
			var lookupId = Guid.Empty;
			var reportId = Guid.Empty;
			var graphicsId = Guid.Empty;
			var workflowId = Guid.Empty;
			var constantId = Guid.Empty;
			foreach(PropertyValueItem property 
			        in ChildItemsByType(PropertyValueItem.CategoryConst))
			{
				if(property.ControlPropertyItem == null)
				{
					continue;
				}
				if((ControlItem.Name == "AsCombo") 
				&& (property.ControlPropertyItem.Name == "LookupId"))
				{
					lookupId = property.GuidValue;
				}
				if((ControlItem.Name == "RadioButton") 
				&& (property.ControlPropertyItem.Name == "DataConstantId"))
				{
					constantId = property.GuidValue;
				}
				if((ControlItem.Name == "AsReportPanel")
				&& (property.ControlPropertyItem.Name == "ReportId"))
				{
					reportId = property.GuidValue;
				}
				if(((ControlItem.Name == "AsPanel")
				    || ControlItem.IsComplexType)
				&& (property.ControlPropertyItem.Name == "IconId"))
				{
					graphicsId = property.GuidValue;
				}
				if((ControlItem.Name == "ExecuteWorkflowButton")
				&& (property.ControlPropertyItem.Name == "IconId"))
				{
					graphicsId = property.GuidValue;
				}
				if((ControlItem.Name == "ExecuteWorkflowButton") 
				&& (property.ControlPropertyItem.Name == "WorkflowId"))
				{
					workflowId = property.GuidValue;
				}
			}
			if(lookupId != Guid.Empty)
			{
				try
				{
					var item = PersistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem), 
						new ModelElementKey(lookupId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException(
						"lookupId", lookupId, ResourceUtils.GetString(
							"ErrorLookupNotFound", Name, RootItem.ItemType, 
							RootItem.Name));
				}
			}
			if(constantId != Guid.Empty)
			{
				try
				{
					var item = PersistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem), 
						new ModelElementKey(constantId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException(
						"constantId", lookupId, ResourceUtils.GetString(
							"ErrorConstantNotFound", Name, RootItem.ItemType, 
							RootItem.Name));
				}
			}
			if(reportId != Guid.Empty)
			{
				try
				{
					var item = PersistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem), 
						new ModelElementKey(reportId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException(
						"reportId", reportId, ResourceUtils.GetString(
							"ErrorReportNotFound", Name, RootItem.ItemType, 
							RootItem.Name));
				}
			}
			if(graphicsId != Guid.Empty)
			{
				try
				{
					var item = PersistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem), 
						new ModelElementKey(graphicsId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("graphicsId", 
						graphicsId, ResourceUtils.GetString(
							"ErrorGraphicsNotFound", Name, RootItem.ItemType, 
							RootItem.Name));
				}
			}
			if(workflowId != Guid.Empty)
			{
				try
				{
					var item = PersistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem), 
						new ModelElementKey(workflowId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("workflowId", 
						workflowId, ResourceUtils.GetString(
							"ErrorWorkflowNotFound", Name, RootItem.ItemType, 
							RootItem.Name));
				}
			}
			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes => new []
		{
			typeof(PropertyValueItem),
			typeof(ControlSetItem),
			typeof(PropertyBindingInfo),
			typeof(ColumnParameterMapping)
		};
		
		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			string itemName = null;
			if(typeof(T) == typeof(PropertyValueItem))
			{
				itemName = "NewPropertyValue";
			}
			else if(typeof(T) == typeof(ControlSetItem))
			{
				itemName = "NewControlSetItem";
			}
			else if(typeof(T) == typeof(PropertyBindingInfo))
			{
				itemName = "NewPropertyBindingInfo";
			}
			else if(typeof(T) == typeof(ColumnParameterMapping))
			{
				itemName = "NewColumnParameterMapping";
			}
			return base.NewItem<T>(schemaExtensionId, group, itemName);
		}

		#endregion
	}

	public class ControlSetItemComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			if(!(x is ControlSetItem xItem))
			{
				throw new ArgumentOutOfRangeException("x", x, 
					"Unsupported type for comparison.");
			}
			if(!(y is ControlSetItem yItem))
			{
				throw new ArgumentOutOfRangeException("y", y, 
					"Unsupported type for comparison.");
			}
			var tabX = TabIndex(xItem);
			var tabY = TabIndex(yItem);
			if(tabX == -1 || tabY == -1)
			{
				return xItem.Name.CompareTo(yItem.Name);
			}
			return tabX.CompareTo(tabY);
		}

		#endregion

		private int TabIndex(ControlSetItem control)
		{
			foreach(PropertyValueItem property in control.ChildItemsByType(
				        PropertyValueItem.CategoryConst))
			{
				if(property.ControlPropertyItem.Name == "TabIndex")
				{
					return property.IntValue;
				}
			}
			return -1;
		}
	}

	public class AlternativeControlSetItemComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			if(!(x is ControlSetItem xItem))
			{
				throw new ArgumentOutOfRangeException ("x", x, 
					"Unsupported type for comparison.");
			}
			if(!(y is ControlSetItem yItem))
			{
				throw new ArgumentOutOfRangeException ("y", y, 
					"Unsupported type for comparison.");
			}
			return xItem.Level.CompareTo (yItem.Level);
		}
		#endregion
	}
}
