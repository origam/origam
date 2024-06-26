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

namespace Origam.Schema.WorkflowModel;
[SchemaItemDescription("(Task) Update context by Xpath", "Tasks", "task-update-context-by-xpath.png")]
[HelpTopic("Update+Context+Task")]
[ClassMetaVersion("6.0.0")]
public class UpdateContextTask : AbstractWorkflowStep
{
	public UpdateContextTask() {}
	public UpdateContextTask(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}
	public UpdateContextTask(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
		XsltDependencyHelper.GetDependencies(
			this, dependencies, ValueXPath);
		dependencies.Add(OutputContextStore);
		dependencies.Add(XPathContextStore);
		dependencies.Add(Entity);
		base.GetExtraDependencies(dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey?.Equals(OutputContextStore.PrimaryKey) 
			== true)
			{
				OutputContextStore = item as IContextStore;
			}
			if(item.OldPrimaryKey?.Equals(XPathContextStore.PrimaryKey) 
			== true)
			{
				XPathContextStore = item as IContextStore;
			}
		}
		base.UpdateReferences ();
	}
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new []
	{
		typeof(WorkflowTaskDependency)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		return base.NewItem<T>(schemaExtensionId, group, 
			typeof(T) == typeof(WorkflowTaskDependency) ?
				"NewWorkflowTaskDependency" : null);
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
			var key = new ModelElementKey
			{
				Id = DataStructureEntityId
			};
			return (DataStructureEntity)PersistenceProvider
				.RetrieveInstance(typeof(DataStructureEntity), key);
		}
		set
		{
			if(value == null)
			{
				DataStructureEntityId = Guid.Empty;
			}
			else
			{
				DataStructureEntityId = (Guid)value.PrimaryKey["Id"];
			}
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
			var key = new ModelElementKey
			{
				Id = OutputContextStoreId
			};
			return (IContextStore)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				OutputContextStoreId = Guid.Empty;
			}
			else
			{
				OutputContextStoreId = (Guid)value.PrimaryKey["Id"];
			}
            // clear entity and field properties only if copying 
            // of whole workflow is not in progress (caller is UpdateReferences())
            if(OldPrimaryKey == null)
            {
                ClearEntityAndField();
            }
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
			var key = new ModelElementKey
			{
				Id = XPathContextStoreId
			};
			return (IContextStore)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				XPathContextStoreId = Guid.Empty;
			}
			else
			{
				XPathContextStoreId = (Guid)value.PrimaryKey["Id"];
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
	
	private void ClearEntityAndField()
	{
		FieldName = null;
		Entity = null;
	}
	public DataStructureColumn GetFieldSchemaItem()
	{
		if(Entity == null)
		{
			return null;
		}
		foreach(DataStructureColumn dataStructureColumn 
		        in Entity.ChildItemsByType(
			        DataStructureColumn.CategoryConst))
		{
			if(dataStructureColumn.Name == FieldName)
			{
				return dataStructureColumn;
			}
		}
		foreach(var dataStructureColumn in Entity.GetColumnsFromEntity())
		{
			if(dataStructureColumn.Name == FieldName)
			{
				return dataStructureColumn;
			}
		}
		return null;
	}
	#endregion
}
	