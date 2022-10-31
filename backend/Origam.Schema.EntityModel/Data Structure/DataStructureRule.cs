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
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DataStructureRule.
	/// </summary>
	[SchemaItemDescription("Rule", "icon_rule.png")]
    [HelpTopic("Rule+Set+Rule")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DataStructureRule : AbstractSchemaItem
	{
		public const string CategoryConst = "DataStructureRule";

		public DataStructureRule() : base(){}
		
		public DataStructureRule(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureRule(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		public ArrayList RuleDependencies
		{
			get
			{
				return this.ChildItemsByType(DataStructureRuleDependency.CategoryConst);
			}
		}

		private int _priority = 100;
		
		[DefaultValue(100)]
        [XmlAttribute("priority")]
		public int Priority
		{
			get
			{
				return _priority;
			}
			set
			{
				_priority = value;
			}
		}
		
		public Guid DataStructureEntityId;

		[TypeConverter(typeof(DataQueryEntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("entity", "DataStructureEntityId")]
        public DataStructureEntity Entity
		{
			get
			{
				return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(DataStructureEntity), new ModelElementKey(this.DataStructureEntityId));
			}
			set
			{
				this.DataStructureEntityId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
				this.TargetField = null;
			}
		}
        
		public Guid TargetFieldId;

		[TypeConverter(typeof(DataStructureEntityFieldConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("targetField", "TargetFieldId")]
		public IDataEntityColumn TargetField
		{
			get
			{
				return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TargetFieldId));
			}
			set
			{
				this.TargetFieldId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid ValueRuleId;

		[TypeConverter(typeof(DataRuleConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("valueRule", "ValueRuleId")]
		public IDataRule ValueRule
		{
			get
			{
				return (IDataRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ValueRuleId));
			}
			set
			{
				this.ValueRuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		public Guid CheckRuleId;

		[TypeConverter(typeof(StartRuleConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("conditionRule", "CheckRuleId")]
		public IStartRule ConditionRule
		{
			get
			{
				return (IStartRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.CheckRuleId));
			}
			set
			{
				this.CheckRuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion

		#region Overriden AbstractSchemaItem Members
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Entity);
			if(this.TargetField != null) dependencies.Add(this.TargetField);
			if(this.ValueRule != null) dependencies.Add(this.ValueRule);
			if(this.ConditionRule != null) dependencies.Add(this.ConditionRule);

			base.GetExtraDependencies (dependencies);
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.Entity.PrimaryKey))
					{
                        IDataEntityColumn targetFieldBck = this.TargetField;
                        // setting the Entity normally resets TargetField as well
						this.Entity = item as DataStructureEntity;
                        this.TargetField = targetFieldBck;
						break;
					}
				}
			}

			base.UpdateReferences ();
		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			return newNode.Equals(this.ParentItem);
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {typeof(DataStructureRuleDependency)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(DataStructureRuleDependency))
			{
				item = new DataStructureRuleDependency(schemaExtensionId);
				item.Name = "NewDependency";

			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDataStructureRuleUnknownType"));

			item.Group = group;
			item.RootProvider = this;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);

			return item;
		}
	#endregion
	}
}
