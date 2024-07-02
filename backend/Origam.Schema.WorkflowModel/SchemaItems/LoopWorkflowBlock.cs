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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for ForeachWorkflowBlock.
/// </summary>
[SchemaItemDescription("(Block) Loop", "Tasks", "block-loop-1.png")]
[HelpTopic("Loop+Block")]
[ClassMetaVersion("6.0.0")]
public class LoopWorkflowBlock : AbstractWorkflowBlock
{
	public LoopWorkflowBlock() : base() {}
	public LoopWorkflowBlock(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public LoopWorkflowBlock(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		XsltDependencyHelper.GetDependencies(this, dependencies, this.LoopConditionXPath);
		dependencies.Add(this.LoopConditionContextStore);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.LoopConditionContextStore.PrimaryKey))
				{
					this.LoopConditionContextStore = item as IContextStore;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	#endregion
	#region Properties
	public Guid ContextStoreId;
	[TypeConverter(typeof(ContextStoreConverter))]
	[NotNullModelElementRule()]
    [XmlReference("loopConditionContextStore", "ContextStoreId")]
	public IContextStore LoopConditionContextStore
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
	string _xpath;
	[StringNotEmptyModelElementRule()]
    [XmlAttribute("loopConditionXPath")]
	public string LoopConditionXPath
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
	#endregion
}
