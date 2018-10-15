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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ContextStore.
	/// </summary>
	[SchemaItemDescription("Context Store", "Context Stores", 17)]
    [HelpTopic("Context+Store")]
    [DefaultProperty("Structure")]
	[XmlModelRoot(ItemTypeConst)]
	public class ContextStore : AbstractSchemaItem, IContextStore
	{
		public const string ItemTypeConst = "ContextStore";

		public ContextStore() : base() {}

		public ContextStore(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ContextStore(Key primaryKey) : base(primaryKey)	{}

		public bool isScalar()
		{
			return this.Structure == null;
		}

		#region Overriden AbstractSchemaItem Members
		
		[EntityColumn("ItemType")]
		public override string ItemType => ItemTypeConst;

		public override string Icon => "17";

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			if(this.Structure != null)
			{
				dependencies.Add(this.Structure);
			}
			if(this.RuleSet != null)
			{
				dependencies.Add(this.RuleSet);
			}
			if(this.DefaultSet != null)
			{
				dependencies.Add(this.DefaultSet);
			}

			base.GetExtraDependencies (dependencies);
		}

		public override SchemaItemCollection ChildItems
		{
			get
			{
				return new SchemaItemCollection();
			}
		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			// can move inside the same workflow and we can move it under any block
			if(this.RootItem == (newNode as ISchemaItem).RootItem && 
				newNode is IWorkflowBlock)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region IContextStore Members
		[DefaultValue(OrigamDataType.Xml)]
		[EntityColumn("I01")]  	
		[XmlAttribute ("dataType")]
		public OrigamDataType DataType { get; set; } = OrigamDataType.Xml;

		[DefaultValue(false)]
		[EntityColumn("B01")]  
		[XmlAttribute ("isReturnValue")]
		public bool IsReturnValue { get; set; } = false;

		[DefaultValue(false)]
		[EntityColumn("B02")]
		[Description("When set to True it will not check for mandatory fields, primary key duplicates or existence of parent records in the in-memory representation of data.")]
		[XmlAttribute ("disableConstraints")]
		public bool DisableConstraints { get; set; } = false;

		[EntityColumn("G01")]  
		public Guid DataStructureId;

		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRule("DataType", null, OrigamDataType.Xml, null)]
		[XmlReference("structure", "DataStructureId")]
		public AbstractDataStructure Structure
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.DataStructureId;

				return (AbstractDataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(this.DataType == OrigamDataType.Xml)
				{
					if(value == null)
					{
						this.DataStructureId = Guid.Empty;
						this.Name = "";
					}
					else
					{
						this.DataStructureId = (Guid)value.PrimaryKey["Id"];

						this.Name = this.Structure.Name;
					}
				}

				this.RuleSet = null;
				this.DefaultSet = null;
			}
		}

		[EntityColumn("G02")]  
		public Guid RuleSetId;

		[TypeConverter(typeof(ContextStoreRuleSetConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("ruleSet", "RuleSetId")]
		public DataStructureRuleSet RuleSet
		{
			get
			{
				return (DataStructureRuleSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RuleSetId));
			}
			set
			{
				this.RuleSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		[EntityColumn("G03")]  
		public Guid DefaultSetId;

		[TypeConverter(typeof(ContextStoreDefaultSetConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("defaultSet", "DefaultSetId")]
		public DataStructureDefaultSet DefaultSet
		{
			get
			{
				return (DataStructureDefaultSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DefaultSetId));
			}
			set
			{
				this.DefaultSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		#endregion
	}
}
