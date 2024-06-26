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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for WorkflowTask.
/// </summary>
[ClassMetaVersion("6.0.0")]
public abstract class WorkflowTask : AbstractWorkflowStep, ISchemaItemFactory, IWorkflowTask
{
	public WorkflowTask() : base() {}
	public WorkflowTask(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public WorkflowTask(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.OutputContextStore);
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
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	#endregion
	#region IWorkflowTask Members
	
	public Guid OutputContextStoreId;
	[TypeConverter(typeof(ContextStoreConverter))]
	[NotNullModelElementRule()]
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
		}
	}
	[DefaultValue(ServiceOutputMethod.AppendMergeExisting)]
	[XmlAttribute ("outputMethod")]
	public virtual ServiceOutputMethod OutputMethod { get; set; } = ServiceOutputMethod.AppendMergeExisting;
	#endregion
}
