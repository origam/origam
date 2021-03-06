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
	public enum WorkflowProperty
	{
		Title,
		Notification,
		ResultMessage
}

	public enum SetWorkflowPropertyMethod
	{
		Overwrite,
		Add
	}

	/// <summary>
	/// Summary description for SetWorkflowPropertyTask.
	/// </summary>
	[SchemaItemDescription("(Task) Set Workflow Property", "Tasks", "task-set-workflow-property.png")]
    [HelpTopic("Set+Workflow+Property+Task")]
    [ClassMetaVersion("6.0.0")]
	public class SetWorkflowPropertyTask : AbstractWorkflowStep, ISchemaItemFactory
	{
		public SetWorkflowPropertyTask() : base() {}

		public SetWorkflowPropertyTask(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public SetWorkflowPropertyTask(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		[EntityColumn("ItemType")]
		public override string ItemType 
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			XsltDependencyHelper.GetDependencies(this, dependencies, this.XPath);

			dependencies.Add(this.ContextStore);

			base.GetExtraDependencies (dependencies);
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.ContextStore.PrimaryKey))
					{
						this.ContextStore = item as IContextStore;
						break;
					}
				}
			}

			base.UpdateReferences ();
		}
		#endregion

		#region Properties
		[EntityColumn("G10")]  
		public Guid ContextStoreId;

		[TypeConverter(typeof(ContextStoreConverter))]
        [XmlReference("contextStore", "ContextStoreId")]
		public IContextStore ContextStore
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ContextStoreId;

				return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.ContextStoreId = Guid.Empty;
				}
				else
				{
					this.ContextStoreId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[EntityColumn("G11")]  
		public Guid TransformationId;

		[TypeConverter(typeof(TransformationConverter))]
        [XmlReference("transformation", "TransformationId")]
		public ITransformation Transformation
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TransformationId)) as ITransformation;
			}
			set
			{
				this.TransformationId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		string _xpath;
		[EntityColumn("LS01")] 
        [XmlAttribute("xPath")]
		public string XPath
		{
			get
			{
				return _xpath;
			}
			set
			{
				_xpath = value;
			}
		}

		string _delimiter = "";
		[EntityColumn("SS03")]
        [XmlAttribute("delimiter")]
        public string Delimiter
		{
			get
			{
				return _delimiter;
			}
			set
			{
				_delimiter = value;
			}
		}

		WorkflowProperty _workflowProperty;
		[EntityColumn("I05")] 
        [XmlAttribute("workflowProperty")]
		public WorkflowProperty WorkflowProperty
		{
			get
			{
				return _workflowProperty;
			}
			set
			{
				_workflowProperty = value;
			}
		}

		SetWorkflowPropertyMethod _setWorkflowPropertyMethod;
		[EntityColumn("I06")] 
        [XmlAttribute("method")]
		public SetWorkflowPropertyMethod Method
		{
			get
			{
				return _setWorkflowPropertyMethod;
			}
			set
			{
				_setWorkflowPropertyMethod = value;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(WorkflowTaskDependency)
								  };
			}
		}

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
	}
}
