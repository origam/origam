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
	/// <summary>
	/// Summary description for ControlSetItem.
	/// </summary>
   [SchemaItemDescription("Alternative", "Alternatives", "icon_alternative.png")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class ControlSetItem  : AbstractSchemaItem, ISchemaItemFactory 
	{

		public const string CategoryConst = "ControlSetItem";

		public ControlSetItem() : base(){}
		
		public ControlSetItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ControlSetItem(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		
		public Guid ControlId;

        [XmlReference("widget", "ControlId")]
		public ControlItem ControlItem
		{
			get
			{
				return (ControlItem)this.PersistenceProvider.RetrieveInstance(typeof(ControlItem), new ModelElementKey(this.ControlId));
			}
			set
			{
				this.ControlId = (Guid)value.PrimaryKey["Id"];
			}
		}
		
		private string _roles;
		[XmlAttribute("roles")]
		public string Roles
		{
			get
			{
				return _roles;
			}
			set
			{
				_roles = value;
			}
		}		

		private string _features;
		[XmlAttribute("features")]
		public string Features
		{
			get
			{
				return _features;
			}
			set
			{
				_features = value;
			}
		}		

		private Guid _multiColumnAdapterFieldCondition;
		
        [XmlAttribute("multiColumnAdapterFieldCondition")]
		public Guid MultiColumnAdapterFieldCondition
		{
			get
			{
				return _multiColumnAdapterFieldCondition;
			}
			set
			{
				_multiColumnAdapterFieldCondition = value;
			}
		}

		private bool _isAlternative = false;

        [XmlAttribute("isAlternative")]
        public bool IsAlternative
		{
			get
			{
				return _isAlternative;
			}
			set
			{
				_isAlternative = value;
			}
		}

        private bool _requestSaveAfterChange = false;

        [XmlAttribute("requestSaveAfterChange")]
        public bool RequestSaveAfterChange
        {
            get
            {
                return _requestSaveAfterChange;
            }
            set
            {
                _requestSaveAfterChange = value;
            }
        }


		private int _level = 100;

        [XmlAttribute("level")]
        public int Level
		{
			get
			{
				return _level;
			}
			set
			{
				_level = value;
			}
		}
		#endregion
		
		#region Overriden AbstractSchemaItem Members
		public override string ItemType
		{
			get
			{
				return ControlSetItem.CategoryConst;
			}
		}

        public override UI.BrowserNodeCollection ChildNodes()
        {
            // return only the 1st level of items (alternative screen/panels) but not child widgets
            if (this.ParentItem.ParentItem == null)
            {
                return new UI.BrowserNodeCollection();
            }
            else
            {
                return base.ChildNodes();
            }
        }

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.ControlItem);

			if(this.ControlItem.PanelControlSet != null)
			{
				dependencies.Add(this.ControlItem.PanelControlSet);
			}

			Guid lookupId = Guid.Empty;
			Guid reportId = Guid.Empty;
			Guid graphicsId = Guid.Empty;
			Guid workflowId = Guid.Empty;

			foreach(PropertyValueItem property in this.ChildItemsByType(PropertyValueItem.CategoryConst))
			{
				if (property.ControlPropertyItem == null) continue;
				if(this.ControlItem.Name == "AsCombo" & property.ControlPropertyItem.Name == "LookupId") lookupId = property.GuidValue;
				if(this.ControlItem.Name == "AsReportPanel" & property.ControlPropertyItem.Name == "ReportId") reportId = property.GuidValue;
				if((this.ControlItem.Name == "AsPanel" | this.ControlItem.IsComplexType) & property.ControlPropertyItem.Name == "IconId") graphicsId = property.GuidValue;
				if(this.ControlItem.Name == "ExecuteWorkflowButton" & property.ControlPropertyItem.Name == "IconId") graphicsId = property.GuidValue;
				if(this.ControlItem.Name == "ExecuteWorkflowButton" & property.ControlPropertyItem.Name == "WorkflowId") workflowId = property.GuidValue;
			}

			if(lookupId != Guid.Empty)
			{
				try
				{
					AbstractSchemaItem item = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(lookupId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("lookupId", lookupId, ResourceUtils.GetString("ErrorLookupNotFound", this.Name, this.RootItem.ItemType, this.RootItem.Name));
				}
			}

			if(reportId != Guid.Empty)
			{
				try
				{
					AbstractSchemaItem item = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(reportId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("reportId", reportId, ResourceUtils.GetString("ErrorReportNotFound", this.Name, this.RootItem.ItemType, this.RootItem.Name));
				}
			}

			if(graphicsId != Guid.Empty)
			{
				try
				{
					AbstractSchemaItem item = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(graphicsId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("graphicsId", graphicsId, ResourceUtils.GetString("ErrorGraphicsNotFound", this.Name, this.RootItem.ItemType, this.RootItem.Name));
				}
			}

			if(workflowId != Guid.Empty)
			{
				try
				{
					AbstractSchemaItem item = this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(workflowId)) as AbstractSchemaItem;
					dependencies.Add(item);
				}
				catch
				{
					throw new ArgumentOutOfRangeException("workflowId", workflowId, ResourceUtils.GetString("ErrorWorkflowNotFound", this.Name, this.RootItem.ItemType, this.RootItem.Name));
				}
			}

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
                return new Type[] { };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(PropertyValueItem))
			{
				PropertyValueItem item = new PropertyValueItem(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewPropertyValue";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else if(type == typeof(ControlSetItem))
			{
				ControlSetItem item = new ControlSetItem(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewControlSetItem";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else if(type == typeof(PropertyBindingInfo))
			{
				PropertyBindingInfo item = new PropertyBindingInfo(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewPropertyBindingInfo";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}	
			else if(type == typeof(ColumnParameterMapping))
			{
				ColumnParameterMapping item = new ColumnParameterMapping(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewColumnParameterMapping";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}

			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorControlSetItemUnknownType"));
		}

		#endregion
	}

	public class ControlSetItemComparer : IComparer
	{
		public ControlSetItemComparer()
		{
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			ControlSetItem xItem = x as ControlSetItem;
			ControlSetItem yItem = y as ControlSetItem;

			if(xItem == null) throw new ArgumentOutOfRangeException("x", x, "Unsupported type for comparison.");
			if(yItem == null) throw new ArgumentOutOfRangeException("y", y, "Unsupported type for comparison.");

			int tabX = TabIndex(xItem);
			int tabY = TabIndex(yItem);

			if(tabX == -1 | tabY == -1)
			{
				return xItem.Name.CompareTo(yItem.Name);
			}
			else
			{
				return tabX.CompareTo(tabY);
			}
		}

		#endregion

		private int TabIndex(ControlSetItem control)
		{
			foreach(PropertyValueItem property in control.ChildItemsByType(PropertyValueItem.CategoryConst))
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
		public AlternativeControlSetItemComparer()
		{
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			ControlSetItem xItem = x as ControlSetItem;
			ControlSetItem yItem = y as ControlSetItem;

			if (xItem == null)
				throw new ArgumentOutOfRangeException ("x", x, "Unsupported type for comparison.");
			if (yItem == null)
				throw new ArgumentOutOfRangeException ("y", y, "Unsupported type for comparison.");

			return xItem.Level.CompareTo (yItem.Level);
		}
		#endregion
	}
}
