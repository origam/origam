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
/// <summary>
/// Summary description for ForeachWorkflowBlock.
/// </summary>
[SchemaItemDescription("(Block) For-each", "Tasks", "block-for-each.png")]
[HelpTopic("For-Each+Block")]
[ClassMetaVersion("6.0.0")]
public class ForeachWorkflowBlock : AbstractWorkflowBlock
{
	public ForeachWorkflowBlock() : base() {}
	public ForeachWorkflowBlock(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ForeachWorkflowBlock(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		XsltDependencyHelper.GetDependencies(this, dependencies, this.IteratorXPath);
		dependencies.Add(this.SourceContextStore);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.SourceContextStore.PrimaryKey))
				{
					this.SourceContextStore = item as IContextStore;
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
    [XmlReference("sourceContextStore", "ContextStoreId")]
	public IContextStore SourceContextStore
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
	[StringNotEmptyModelElementRule]
    [XmlAttribute("iteratorXPath")]
	public string IteratorXPath
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
	bool _ignoreSourceContextChanges = false;
	
	[DefaultValue(false)]
    [XmlAttribute("ignoreSourceContextChanges")]
	public bool IgnoreSourceContextChanges
	{
		get
		{
			return _ignoreSourceContextChanges;
		}
		set
		{
			_ignoreSourceContextChanges = value;
		}
	}
	#endregion
}
