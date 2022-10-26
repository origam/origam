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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for UpdateContextTask.
	/// </summary>
	[SchemaItemDescription("(Task) Update context by Xpath", "Tasks", "task-update-context-by-xpath.png")]
    [HelpTopic("Update+Context+Task")]
    [ClassMetaVersion("6.0.0")]
	public class UpdateContextTask : AbstractWorkflowStep, ISchemaItemFactory
	{
		public UpdateContextTask() : base()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public UpdateContextTask(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public UpdateContextTask(Key primaryKey) : base(primaryKey)	{}
		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType => CategoryConst;

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			XsltDependencyHelper.GetDependencies(this, dependencies, this.ValueXPath);

			dependencies.Add(this.OutputContextStore);
			dependencies.Add(this.XPathContextStore);
			dependencies.Add(this.Entity);

			base.GetExtraDependencies (dependencies);
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.OutputContextStore.PrimaryKey))
					{
						this.OutputContextStore = item as IContextStore;
					}
					if(item.OldPrimaryKey.Equals(this.XPathContextStore.PrimaryKey))
					{
						this.XPathContextStore = item as IContextStore;
					}
				}
			}

			base.UpdateReferences ();
		}
		#endregion


		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes =>
			new [] {typeof(WorkflowTaskDependency)};

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(WorkflowTaskDependency))
			{
				item = new WorkflowTaskDependency(schemaExtensionId);
				item.Name = "NewWorkflowTaskDependency";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorSetWorkflowPropertyTaskUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}
		#endregion

		#region Properties

		public Guid DataStructureEntityId;

		[TypeConverter(typeof(ContextStoreEntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[Category("Output")]
		[Description("A data structure entity within `OutputContextStore' which is to be updated. `Entity' is only applicable if an `OutputContextStore'"
				+ " is a data structure context store.")]
		[XmlReference("entity", "DataStructureEntityId")]
		public DataStructureEntity Entity
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.DataStructureEntityId;

				return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(DataStructureEntity), key);
			}
			set
			{
				if(value == null)
				{
					this.DataStructureEntityId = Guid.Empty;
				}
				else
				{
					this.DataStructureEntityId = (Guid)value.PrimaryKey["Id"];
				}

				//UpdateName();
			}
		}

		[UpdateContextTaskValidModelElementRuleAttribute()]
		[Category("Output"), RefreshProperties(RefreshProperties.Repaint)]
		[Description("A Name of a field (column) within `Entity' within an `OutputContextStore' which is to be updated for all rows of `Entity'"
				+ " with a return value of `ValueXPath'. `FieldName' is applicable only if an OutputContextStore is a data struture context store.")]
		[XmlAttribute ("fieldName")]
		public string FieldName { get; set; }
		
		public Guid OutputContextStoreId;
		[TypeConverter(typeof(ContextStoreConverter))]
		[Description("Context store to be updated. In case of simple scalar context the value of context is updated with a result value of `ValueXPath'."
						+ " In case of data structure context store `FieldName' is updated for all columns of `Entity'.")]
		[Category("Output")]
		[NotNullModelElementRuleAttribute()]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("outputContextStore", "OutputContextStoreId")]
		public IContextStore OutputContextStore
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.OutputContextStoreId;

				return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.OutputContextStoreId = Guid.Empty;
				}
				else
				{
					this.OutputContextStoreId = (Guid)value.PrimaryKey["Id"];
				}
                // clear entity and field properties only if copying 
                // of whole workflow is not in progress (caller is UpdateReferences())
                if (this.OldPrimaryKey == null) this.clearEntityAndField();
			}
		}

		public Guid XPathContextStoreId;

		[TypeConverter(typeof(ContextStoreConverter))]
		[Category("Input")]
		[Description("Contextstore to perform a ValueXPath expression on.")]
		[NotNullModelElementRuleAttribute()]
		[XmlReference("xPathContextStore", "XPathContextStoreId")]
		public IContextStore XPathContextStore
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.XPathContextStoreId;

				return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.XPathContextStoreId = Guid.Empty;
				}
				else
				{
					this.XPathContextStoreId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[Category("Input")]
		[Description("Result of this XPath is a value which is to be used fo updating a OutputContextStore.")]
		[StringNotEmptyModelElementRule()]
		[XmlAttribute ("valueXPath")]
		public string ValueXPath { get; set; }
		#endregion

		#region Private Methods
		
		private void clearEntityAndField()
		{
			this.FieldName = null;
			this.Entity = null;
		}
		

		public DataStructureColumn GetFieldSchemaItem()
		{
			if (this.Entity == null) return null;
			foreach (DataStructureColumn col in
				Entity.ChildItemsByType(DataStructureColumn.CategoryConst))
			{
				if (col.Name == FieldName)
				{
					return col;
				}
			}

			foreach (DataStructureColumn col in 
				Entity.GetColumnsFromEntity())
			{
				if (col.Name == FieldName)
				{
					return col;
				}
			}
			return null;
		}
		#endregion
	}
}





		